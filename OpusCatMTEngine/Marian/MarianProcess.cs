using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Management;
using System.Text.RegularExpressions;
using Serilog;
using System.Data.SQLite;
using System.Data;
using System.Windows.Controls.Primitives;
using System.ServiceModel.Description;

namespace OpusCatMTEngine
{
    public class MarianProcess
    {
        /*private Process tokenizeAndTruecaseProcess;
        private Process bpeProcess;
        private Process decoderProcess;*/
        private Process mtPipe;
        private string langpair;
        
        private volatile bool translationInProgress = false;

        public string SourceCode { get; }
        public string TargetCode { get; }
        public bool Faulted { get; private set; }
        public Process MtPipe { get => mtPipe; set => mtPipe = value; }

        private ConcurrentStack<Task<string>> taskStack = new ConcurrentStack<Task<string>>();

        private StreamWriter utf8Writer;
        private string modelDir;
        private readonly bool includePlaceholderTags;
        private readonly bool includeTagPairs;

        public string SystemName { get; }

        private string mtPipeCmds;
        private bool sentencePiecePostProcess;
        private static readonly Object lockObj = new Object();

        //This seems like the only way to cleanly close the NMT window when Studio is closing.
        //On some systems the unclean closing was causing an error pop-up, that's why this was added.
        //On my dev system the was no pop-up, but the error was reported in Event Viewer.
        private static void KillProcessAndChildren(int pid)
        {
            // Cannot close 'system idle process'.
            if (pid == 0)
            {
                return;
            }
            ManagementObjectSearcher searcher = new ManagementObjectSearcher
                    ("Select * From Win32_Process Where ParentProcessID=" + pid);
            ManagementObjectCollection moc = searcher.Get();
            foreach (ManagementObject mo in moc)
            {
                KillProcessAndChildren(Convert.ToInt32(mo["ProcessID"]));
            }
            try
            {
                Process proc = Process.GetProcessById(pid);
                proc.Kill();
            }
            catch (ArgumentException)
            {
                // Process already exited.
            }
        }

        public MarianProcess(
            string modelDir, 
            string sourceCode, 
            string targetCode, 
            bool includePlaceholderTags,
            bool includeTagPairs)
        {
            this.Faulted = false;
            this.langpair = $"{sourceCode}-{targetCode}";
            this.SourceCode = sourceCode;
            this.TargetCode = targetCode;
            this.includePlaceholderTags = includePlaceholderTags;
            this.includeTagPairs = includeTagPairs;
            this.modelDir = modelDir;
            this.SystemName = $"{sourceCode}-{targetCode}_" + (new DirectoryInfo(this.modelDir)).Name;
         
            Log.Information($"Starting MT pipe for model {this.SystemName}.");
            //Both moses+BPE and sentencepiece preprocessing are supported, check which one model is using
            if (Directory.GetFiles(this.modelDir).Any(x=> new FileInfo(x).Name == "source.spm"))
            {
                this.mtPipeCmds = "StartSentencePieceMtPipe.bat";
                this.sentencePiecePostProcess = true;
            }
            else
            {
                this.mtPipeCmds = "StartMosesBpeMtPipe.bat";
                this.sentencePiecePostProcess = false;
            }

            //this.MtPipe = MarianHelper.StartProcessInBackgroundWithRedirects(this.mtPipeCmds, this.modelDir);
            this.MtPipe = MarianHelper.StartProcessInBackgroundWithRedirects(this.mtPipeCmds, this.modelDir);

            this.utf8Writer = new StreamWriter(this.MtPipe.StandardInput.BaseStream, new UTF8Encoding(false));

            AppDomain.CurrentDomain.ProcessExit += new EventHandler(CurrentDomain_ProcessExit);
        }


        private void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            this.ShutdownMtPipe();
        }

        internal List<string> BatchTranslate(List<string> input)
        {
            throw new NotImplementedException();
        }

        
        private void errorDataHandler(object sender, DataReceivedEventArgs e)
        {
            Log.Information(e.Data);
        }

        public void ShutdownMtPipe()
        {
            KillProcessAndChildren(this.mtPipe.Id);
            //Remove the event handler so it doesn't try to kill an already killed process
            AppDomain.CurrentDomain.ProcessExit -= CurrentDomain_ProcessExit;
        }
        
        private string TranslateSentence(string rawSourceSentence)
        {
            //This preprocessing must correspond to the one used in model training. Currently
            //this is:
            //${TOKENIZER}/replace-unicode-punctuation.perl |
            //${ TOKENIZER}/ remove - non - printing - char.perl |
            //${ TOKENIZER}/ normalize - punctuation.perl - l $1 |
            //sed 's/  */ /g;s/^ *//g;s/ *$$//g' |

            var sourceSentence = MarianHelper.PreprocessLine(rawSourceSentence, this.TargetCode, this.includePlaceholderTags, this.includeTagPairs);

            this.utf8Writer.WriteLine(sourceSentence);
            this.utf8Writer.Flush();

            //There should only ever be a single line in the stdout, since there's only one line of
            //input per stdout readline, and marian decoder will never insert line breaks into translations.
            string translation = this.MtPipe.StandardOutput.ReadLine();

            if (this.sentencePiecePostProcess)
            {
                translation = (translation.Replace(" ", "")).Replace("▁", " ").Trim();
            }

            return translation;
        }

        public Task<string> AddToTranslationQueue(string sourceText)
        {
            string existingTranslation = TranslationDbHelper.FetchTranslationFromDb(sourceText, this.SystemName);
            if (existingTranslation != null)
            {
                //If translation already exists, just return it
                return Task.Run(() => existingTranslation);
            }
            else
            {
                Task<string> translationTask = new Task<string>(() => Translate(sourceText));
                translationTask.ContinueWith((x) => CheckTaskStack());
                
                //if there's no translation in progress, start translation
                
                if (this.translationInProgress)
                {
                    this.taskStack.Push(translationTask);
                }
                else
                {
                    this.translationInProgress = true;
                    translationTask.Start();
                }
                
                return translationTask;
            }
        }

        private void CheckTaskStack()
        {
            Task<string> translationTask;
            var success = this.taskStack.TryPop(out translationTask);

            if (success)
            {
                translationTask.Start();
            }
        }

        public string Translate(string sourceText)
        {
            //This is used to determine whether a incoming translation should be immediately started
            //or queued.
            //This isn't thread-safe, but the even if there's race conditions etc., this will only
            //have a minor effect on the order of the translations, it won't break anything.
            //Main point is that there will not be a case where a translation is not started or queued
            //when the request arrives (this should be guaranteed by the code flow).
            translationInProgress = true;

            if (this.MtPipe.HasExited)
            {
                throw new Exception("Opus MT functionality has stopped working. Restarting the OPUS-CAT MT Engine may resolve the problem.");
            }

            string existingTranslation = TranslationDbHelper.FetchTranslationFromDb(sourceText,this.SystemName);
            if (existingTranslation != null)
            {
                translationInProgress = false;
                return existingTranslation;
            }
            else
            {
                lock (MarianProcess.lockObj)
                {
                    /*Stopwatch sw = new Stopwatch();
                    sw.Start();*/
                    //Check again if the translation has been produced during the lock
                    //waiting period
                    existingTranslation = TranslationDbHelper.FetchTranslationFromDb(sourceText, this.SystemName);
                    if (existingTranslation != null)
                    {
                        translationInProgress = false;
                        return existingTranslation;
                    }

                    //It might be the case that the source text contains multiple sentences,
                    //potentially even line breaks, so the text needs to be split on line breaks.
                    //(sentence splitting might be nice, but having multiple sentences on one line
                    //doesn't break anything, while multiple lines cause desyncing problems.
                    var splitSource = new List<string> { sourceText };// sourceText.Split(new[] {"\r\n","\r","\n"},StringSplitOptions.None);

                    StringBuilder translationBuilder = new StringBuilder();
                    foreach (var sourceSentence in splitSource)
                    {
                        translationBuilder.Append(this.TranslateSentence(sourceSentence));
                    }

                    var translation = translationBuilder.ToString();
                
                    TranslationDbHelper.WriteTranslationToDb(sourceText, translation, this.SystemName);
                    //sw.Stop();

                    translationInProgress = false;
                    return translation;
                }
            }
            
        }

    }

}
    