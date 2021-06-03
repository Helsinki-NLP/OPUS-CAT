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
        
        private volatile bool translationInProgress = false;

        public string SourceCode { get; set; }
        public string TargetCode { get; set; }
        public bool Faulted { get; private set; }
        public Process MtProcess { get => mtPipe; set => mtPipe = value; }
        private Process preprocessPipe;

        private ConcurrentStack<Task<TranslationPair>> taskStack = new ConcurrentStack<Task<TranslationPair>>();

        private StreamWriter utf8PreprocessWriter;
        private StreamWriter utf8MtWriter;
        private string modelDir;
        private readonly bool includePlaceholderTags;
        private readonly bool includeTagPairs;

        public string SystemName { get; }
        public bool MultilingualModel { get; private set; }
        public Process PreprocessProcess { get => preprocessPipe; set => preprocessPipe = value; }
        public Process PostprocessProcess { get; private set; }
        private StreamWriter utf8PostprocessWriter;

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
            string modelName, 
            bool multilingualModel,
            bool includePlaceholderTags,
            bool includeTagPairs)
        {
            this.Faulted = false;
            this.SourceCode = sourceCode;
            this.TargetCode = targetCode;
            this.includePlaceholderTags = includePlaceholderTags;
            this.includeTagPairs = includeTagPairs;
            this.modelDir = modelDir;
            this.SystemName = $"{this.SourceCode}-{this.TargetCode}_{modelName}";
            this.MultilingualModel = multilingualModel;

            Log.Information($"Starting MT pipe for model {this.SystemName}.");
            //Both moses+BPE and sentencepiece preprocessing are supported, check which one model is using
            //There are scripts for monolingual and multilingual models

            string preprocessCommand, mtCommand;

            if (Directory.GetFiles(this.modelDir).Any(x=> new FileInfo(x).Name == "source.spm"))
            {
                preprocessCommand = $@"Preprocessing\spm_encode.exe --model {this.modelDir}\source.spm";
                this.sentencePiecePostProcess = true;
            }
            else
            {
                preprocessCommand = $@"Preprocessing\mosesprocessor.exe --stage preprocess --sourcelang {this.SourceCode} --tcmodel {this.modelDir}\source.tcmodel";
                this.sentencePiecePostProcess = false;
                var postprocessCommand =
                    $@"Preprocessing\mosesprocessor.exe --stage postprocess --targetlang {this.TargetCode}";
                this.PostprocessProcess = MarianHelper.StartProcessInBackgroundWithRedirects(postprocessCommand);
                this.utf8PostprocessWriter =
                    new StreamWriter(this.PostprocessProcess.StandardInput.BaseStream, new UTF8Encoding(false));
            }

            mtCommand = $@"Marian\marian.exe decode --log-level=warn -c {this.modelDir}\decoder.yml --max-length=200 --max-length-crop --alignment=hard";

            //this.MtPipe = MarianHelper.StartProcessInBackgroundWithRedirects(this.mtPipeCmds, this.modelDir);
            this.PreprocessProcess = 
                MarianHelper.StartProcessInBackgroundWithRedirects(preprocessCommand);
            this.MtProcess = 
                MarianHelper.StartProcessInBackgroundWithRedirects(mtCommand);
            
            this.utf8PreprocessWriter =
                new StreamWriter(this.PreprocessProcess.StandardInput.BaseStream, new UTF8Encoding(false));
            this.utf8MtWriter =
                new StreamWriter(this.MtProcess.StandardInput.BaseStream, new UTF8Encoding(false));

            AppDomain.CurrentDomain.ProcessExit += new EventHandler(CurrentDomain_ProcessExit);
        }
        

        private void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            this.ShutdownMtPipe();
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
        
        private TranslationPair TranslateSentence(string rawSourceSentence)
        {
            //This preprocessing must correspond to the one used in model training. Currently
            //this is:
            //${TOKENIZER}/replace-unicode-punctuation.perl |
            //${ TOKENIZER}/ remove - non - printing - char.perl |
            //${ TOKENIZER}/ normalize - punctuation.perl - l $1 |
            //sed 's/  */ /g;s/^ *//g;s/ *$$//g' |

            var sourceSentence = MarianHelper.PreprocessLine(rawSourceSentence, this.SourceCode, this.includePlaceholderTags, this.includeTagPairs);

            //Add preprocessing in here, capture preprocessed source for getting aligned tokens.

            this.utf8PreprocessWriter.WriteLine(sourceSentence);
            this.utf8PreprocessWriter.Flush();

            string preprocessedLine = this.PreprocessProcess.StandardOutput.ReadLine();

            if (this.MultilingualModel)
            {
                preprocessedLine = $">>{this.TargetCode}<< {preprocessedLine}";
            }

            this.utf8MtWriter.WriteLine(preprocessedLine);
            this.utf8MtWriter.Flush();

            //There should only ever be a single line in the stdout, since there's only one line of
            //input per stdout readline, and marian decoder will never insert line breaks into translations.
            string translationAndAlignment = this.MtProcess.StandardOutput.ReadLine();

            TranslationPair alignedTranslationPair = new TranslationPair(preprocessedLine, translationAndAlignment);

            if (this.sentencePiecePostProcess)
            {
                alignedTranslationPair.Translation = alignedTranslationPair.RawTranslation.Replace(" ","").Replace("▁", " ").Trim();
                alignedTranslationPair.Segmentation = SegmentationMethod.SentencePiece;
            }
            else
            {
                this.utf8PostprocessWriter.WriteLine(alignedTranslationPair.RawTranslation);
                this.utf8PostprocessWriter.Flush();
                string postprocessedTranslation = this.PostprocessProcess.StandardOutput.ReadLine();
                alignedTranslationPair.Translation = postprocessedTranslation;
                alignedTranslationPair.Segmentation = SegmentationMethod.Bpe;
            }

            return alignedTranslationPair;
            
        }

        

        public Task<TranslationPair> AddToTranslationQueue(string sourceText)
        {
            TranslationPair existingTranslation = TranslationDbHelper.FetchTranslationFromDb(sourceText, this.SystemName);
            if (existingTranslation != null)
            {
                //If translation already exists, just return it
                return Task.Run(() => existingTranslation);
            }
            else
            {
                Task<TranslationPair> translationTask = new Task<TranslationPair>(() => Translate(sourceText));
                translationTask.ContinueWith((x) => CheckTaskStack());

                //if there's translation in progress, push the task to stack to wait
                //otherwise start translation
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
            Task<TranslationPair> translationTask;
            var success = this.taskStack.TryPop(out translationTask);

            if (success)
            {
                translationTask.Start();
            }
        }

        public TranslationPair Translate(string sourceText)
        {
            //This is used to determine whether a incoming translation should be immediately started
            //or queued.
            //This isn't thread-safe, but the even if there's race conditions etc., this will only
            //have a minor effect on the order of the translations, it won't break anything.
            //Main point is that there will not be a case where a translation is not started or queued
            //when the request arrives (this should be guaranteed by the code flow).
            translationInProgress = true;

            if (this.MtProcess.HasExited)
            {
                throw new Exception("Opus MT functionality has stopped working. Restarting the OPUS-CAT MT Engine may resolve the problem.");
            }

            TranslationPair existingTranslation = TranslationDbHelper.FetchTranslationFromDb(sourceText,this.SystemName);
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
                    
                    
                    var translation = this.TranslateSentence(sourceText);
                    
                    TranslationDbHelper.WriteTranslationToDb(sourceText, translation, this.SystemName);
                    //sw.Stop();

                    translationInProgress = false;
                    return translation;
                }
            }
            
        }

    }

}
    