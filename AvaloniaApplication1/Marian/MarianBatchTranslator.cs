using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Serilog;
using System.Data;
using YamlDotNet.Serialization;


namespace OpusCatMtEngine
{
    public class MarianBatchTranslator
    {
        public event EventHandler OutputReady;

        protected virtual void OnOutputReady(EventArgs e)
        {
            EventHandler handler = OutputReady;
            handler?.Invoke(this, e);
        }

        private string langpair;

        public string SourceCode { get; }
        public string TargetCode { get; }
        
        private DirectoryInfo modelDir;
        
        public string SystemName { get; }

        private bool includePlaceholderTags;
        private bool includeTagPairs;
        private SegmentationMethod segmentation;

        private void WriteToTranslationDb(object sender, EventArgs e, IEnumerable<string> input, FileInfo spInput, FileInfo transAndAlign)
        {
            Queue<string> inputQueue
                = new Queue<string>(input);

            if (transAndAlign.Exists)
            {
                using (var reader = transAndAlign.OpenText())
                {
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        var sourceLine = inputQueue.Dequeue();
                        var transPair = new TranslationPair(sourceLine, line, this.segmentation, this.TargetCode);
                        TranslationDbHelper.WriteTranslationToDb(sourceLine, transPair, this.SystemName, this.segmentation, this.TargetCode);
                    }
                }
            }
            
        }

        public MarianBatchTranslator(
            string modelDir, 
            IsoLanguage sourceLang,
            IsoLanguage targetLang,
            SegmentationMethod segmentation,
            bool includePlaceholderTags, 
            bool includeTagPairs)
        {
            this.SourceCode = sourceLang.OriginalCode;
            this.TargetCode = targetLang.OriginalCode;
            this.segmentation = segmentation;

            this.includePlaceholderTags = includePlaceholderTags;
            this.includeTagPairs = includeTagPairs;
            this.modelDir = new DirectoryInfo(modelDir);
            this.SystemName = $"{this.SourceCode}-{this.TargetCode}_" + this.modelDir.Name;
            
            //Check if batch.yml exists, if not create it from decode.yml
            var batchYaml = this.modelDir.GetFiles("batch.yml");
            if (batchYaml.Length == 0)
            {
                var decoderYaml = this.modelDir.GetFiles("decoder.yml").Single();
                var deserializer = new Deserializer();
                var decoderSettings = deserializer.Deserialize<MarianDecoderConfig>(decoderYaml.OpenText());
                decoderSettings.miniBatch = "16";
                decoderSettings.log = Path.Combine(this.modelDir.FullName,"batch.log");
                decoderSettings.alignment = "hard";

                var serializer = new Serializer();
                var configPath = Path.Combine(this.modelDir.FullName, "batch.yml");
                using (var writer = File.CreateText(configPath))
                {
                    serializer.Serialize(writer, decoderSettings, typeof(MarianDecoderConfig));
                }
            }

        }

        
        
        
        internal Process BatchTranslate(
            IEnumerable<string> input,
            FileInfo spOutput,
            Boolean preprocessedInput=false,
            Boolean storeTranslations=false)
        {
            
            Log.Information($"Starting batch translator for model {this.SystemName}.");

            var srcFile = MarianHelper.LinesToFile(input, this.SourceCode);
            FileInfo spInput;
            if (!preprocessedInput)
            {
                
                FileInfo sourceSegModel =
                    this.modelDir.GetFiles().Where(x => Regex.IsMatch(x.Name, "source.(spm|bpe)")).Single();

                spInput = MarianHelper.PreprocessLanguage(
                    srcFile,
                    new DirectoryInfo(Path.GetTempPath()),
                    this.TargetCode,
                    sourceSegModel,
                    this.includePlaceholderTags,
                    this.includeTagPairs);
            }
            else
            {
                spInput = srcFile;
            }

            //TODO: check the translation cache for translations beforehand, and only translate new
            //segments (also change translation cache to account for different decoder configs for
            //same systems, i.e. keep track of decoder settings)

            FileInfo transAndAlign = new FileInfo($"{spOutput.FullName}.transandalign");
            var args = $"\"{this.modelDir.FullName}\" \"{spInput.FullName}\" \"{transAndAlign.FullName}\" --log-level=info --quiet";

            if (storeTranslations)
            {
                this.OutputReady += (x, y) => this.WriteToTranslationDb(x, y, input, spInput, transAndAlign);
            }
            
            EventHandler exitHandler = (x, y) => BatchProcess_Exited(transAndAlign, spOutput, x, y);

            var cmd = "TranslateBatchSentencePiece.bat";
            var batchProcess = MarianHelper.StartProcessInBackgroundWithRedirects(cmd, args, exitHandler);
            

            return batchProcess;
        }

        private void BatchProcess_Exited(
            FileInfo transAndAlignOutput,
            FileInfo spOutput,
            object sender,
            EventArgs e)
        {
            
            Log.Information($"Batch translation process for model {this.SystemName} exited. Processing output.");
            
            if (transAndAlignOutput.Exists)
            {
                FileInfo alignmentFile = new FileInfo($"{spOutput.FullName}.alignments");
                using (var reader = transAndAlignOutput.OpenText())
                using (var alignmentWriter = alignmentFile.CreateText())
                using (var translationWriter = spOutput.CreateText())
                {
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        //Output is translation ||| alignments
                        var transAndAlign = line.Split(new string[] { " ||| " }, StringSplitOptions.None);
                        var transline = transAndAlign[0];
                        translationWriter.WriteLine(transline);
                        alignmentWriter.WriteLine(transAndAlign[1]);
                        
                    }
                }
            }

            this.OnOutputReady(e);
        }


        private void errorDataHandler(object sender, DataReceivedEventArgs e)
        {
            Log.Information(e.Data);
        }
        
    }
    
}
    