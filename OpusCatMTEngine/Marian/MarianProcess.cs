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

        private ConcurrentStack<Task<TranslationPair>> taskStack = new ConcurrentStack<Task<TranslationPair>>();

        private StreamWriter utf8MtWriter;
        private string modelDir;
        private readonly bool includePlaceholderTags;
        private readonly bool includeTagPairs;

        public string SystemName { get; }
        public bool TargetLanguageCodeRequired { get; private set; }

        public IPreprocessor preprocessor;
        
        private SegmentationMethod segmentation;
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
            this.TargetLanguageCodeRequired = multilingualModel;

            Log.Information($"Starting MT pipe for model {this.SystemName}.");
            //Both moses+BPE and sentencepiece preprocessing are supported, check which one model is using
            //There are scripts for monolingual and multilingual models



            if (Directory.GetFiles(this.modelDir).Any(x=> new FileInfo(x).Name == "source.spm"))
            {
                this.preprocessor = new SentencePiecePreprocessor($"{this.modelDir}\\source.spm", $"{this.modelDir}\\target.spm");
                //preprocessCommand = $"Preprocessing\\spm_encode.exe --model \"{this.modelDir}\\source.spm\"";
                
                this.segmentation = SegmentationMethod.SentencePiece;
            }
            else
            {
                this.preprocessor = 
                    new MosesBpePreprocessor(
                        $"{this.modelDir}\\source.tcmodel",
                        $"{this.modelDir}\\source.bpe",
                        this.SourceCode, this.TargetCode);

                this.segmentation = SegmentationMethod.Bpe;
            }

            //mtCommand = $"Marian\\marian.exe decode --log-level=warn -c \"{this.modelDir}\\decoder.yml\" --max-length=200 --max-length-crop --alignment=hard";
            
            string mtCommand = "Marian\\marian.exe";
            string mtArgs = $"decode --log-level=warn -c \"{this.modelDir}\\decoder.yml\" --max-length={OpusCatMTEngineSettings.Default.MaxLength} --max-length-crop --alignment=hard";

            //this.MtPipe = MarianHelper.StartProcessInBackgroundWithRedirects(this.mtPipeCmds, this.modelDir);
            /*this.PreprocessProcess = 
                MarianHelper.StartProcessDirectlyInBackgroundWithRedirects(preprocessCommand,preprocessArgs);*/
            this.MtProcess = 
                MarianHelper.StartProcessDirectlyInBackgroundWithRedirects(mtCommand,mtArgs);
            
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

            //this.utf8PreprocessWriter.WriteLine(sourceSentence);
            //this.utf8PreprocessWriter.Flush();

            //string preprocessedLine = this.PreprocessProcess.StandardOutput.ReadLine();
            string preprocessedLine = this.preprocessor.PreprocessSentence(sourceSentence);

            var lineToTranslate = preprocessedLine;
            if (this.TargetLanguageCodeRequired)
            {
                lineToTranslate = $">>{this.TargetCode}<< {preprocessedLine}";
            }

            this.utf8MtWriter.WriteLine(lineToTranslate);
            this.utf8MtWriter.Flush();

            //There should only ever be a single line in the stdout, since there's only one line of
            //input per stdout readline, and marian decoder will never insert line breaks into translations.
            string translationAndAlignment = this.MtProcess.StandardOutput.ReadLine();

            TranslationPair alignedTranslationPair = new TranslationPair(preprocessedLine, translationAndAlignment, this.segmentation, this.TargetCode);

            //If the source sentence is long (over 150 units) and the translation is much shorter, it's likely
            //that the translation is a fragment or otherwise corrupted. Split the source sentence in two, translate
            //the separate bits, and join the translations to generate a non-fragment translation. This works
            //recursively, so the two halfs may be further split.
            if (OpusCatMTEngineSettings.Default.FixUnbalancedLongTranslations)
            {
                if (alignedTranslationPair.SegmentedSourceSentence.Length > OpusCatMTEngineSettings.Default.UnbalancedSplitMinLength &&
                    (alignedTranslationPair.SegmentedTranslation.Length * OpusCatMTEngineSettings.Default.UnbalancedSplitLengthRatio <
                    alignedTranslationPair.SegmentedSourceSentence.Length))
                {
                    List<string> sourceSplit = this.SplitSentence(rawSourceSentence);

                    if (sourceSplit != null && sourceSplit.Count == 2)
                    {
                        var splitTranslations = sourceSplit.Select(TranslateSentence).ToList();
                        splitTranslations.First().AppendTranslationPair(splitTranslations.Last());
                        alignedTranslationPair = splitTranslations.First();
                    }
                }
            }

            //If the translation pair is merged from split source, it will already have a translation
            if (String.IsNullOrWhiteSpace(alignedTranslationPair.Translation))
            {
                alignedTranslationPair.Translation = this.preprocessor.PostprocessSentence(alignedTranslationPair.RawTranslation);
            }

            return alignedTranslationPair;
        }

        private List<string> SplitOnMiddle(string preprocessedLine, string splitPattern)
        {
            var lineLength = preprocessedLine.Length;
            var splitterMatches = Regex.Matches(preprocessedLine, Regex.Escape(splitPattern)).Cast<Match>().ToList();

            //Filter out very uneven splits (where one split is less than third of line length), since those won't solve the problem
            var midpoint = lineLength / 2;
            splitterMatches = splitterMatches.Where(x => Math.Abs(midpoint - x.Index) < (lineLength / 3)).ToList();

            if (splitterMatches.Any())
            {
                var splitPoint = 
                    splitterMatches.OrderBy(x => Math.Abs((lineLength / 2) - x.Index)).First();
                return new List<string>()
                {
                    preprocessedLine.Substring(0,splitPoint.Index),
                    preprocessedLine.Substring(splitPoint.Index)
                };
            }

            return null;
        }

        private List<string> SplitSentence(string preprocessedLine)
        {
            foreach (var splitPattern in OpusCatMTEngineSettings.Default.UnbalancedSplitPatterns)
            {
                var splitResults = this.SplitOnMiddle(preprocessedLine, splitPattern);
                if (splitResults != null)
                {
                    return splitResults;
                }
            }

            return null;
        }

        public Task<TranslationPair> AddToTranslationQueue(string sourceText)
        {
            TranslationPair existingTranslation = TranslationDbHelper.FetchTranslationFromDb(sourceText, this.SystemName, this.TargetCode);
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

            TranslationPair existingTranslation = TranslationDbHelper.FetchTranslationFromDb(sourceText,this.SystemName,this.TargetCode);
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
                    existingTranslation = TranslationDbHelper.FetchTranslationFromDb(sourceText, this.SystemName, this.TargetCode);
                    if (existingTranslation != null)
                    {
                        translationInProgress = false;
                        return existingTranslation;
                    }
                    
                    
                    var translation = this.TranslateSentence(sourceText);
                    
                    TranslationDbHelper.WriteTranslationToDb(sourceText, translation, this.SystemName, this.segmentation, this.TargetCode);
                    //sw.Stop();

                    translationInProgress = false;
                    return translation;
                }
            }
            
        }

    }

}
    