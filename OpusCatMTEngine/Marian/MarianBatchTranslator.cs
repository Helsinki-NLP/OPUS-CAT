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
using YamlDotNet.Serialization;
using Microsoft.Win32;

namespace OpusCatMTEngine
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

        private void WriteToTranslationDb(object sender, EventArgs e, IEnumerable<string> input, FileInfo spOutput)
        {
            Queue<string> inputQueue
                = new Queue<string>(input);

            if (spOutput.Exists)
            {
                using (var reader = spOutput.OpenText())
                {
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        var nonSpLine = (line.Replace(" ", "")).Replace("▁", " ").Trim();
                        var sourceLine = inputQueue.Dequeue();
                        TranslationDbHelper.WriteTranslationToDb(sourceLine, nonSpLine, this.SystemName);
                    }
                }
            }
            
        }

        public MarianBatchTranslator(
            string modelDir, 
            IsoLanguage sourceLang,
            IsoLanguage targetLang, 
            bool includePlaceholderTags, 
            bool includeTagPairs)
        {
            this.SourceCode = sourceLang.ShortestIsoCode;
            this.TargetCode = targetLang.ShortestIsoCode;
            
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
        
        //Callback can be used to do different things with translation output/input (default is to save in translation cache)
        internal Process BatchTranslate(
            IEnumerable<string> input,
            FileInfo spOutput,
            Boolean preprocessedInput=false,
            Boolean storeTranslations=false)
        {
            if (storeTranslations)
            {
                this.OutputReady += (x, y) => this.WriteToTranslationDb(x, y, input, spOutput);
            }

            Log.Information($"Starting batch translator for model {this.SystemName}.");
            
            var cmd = "TranslateBatchSentencePiece.bat";
                        
            FileInfo spInput = this.PreprocessInput(input,preprocessedInput);

            //TODO: check the translation cache for translations beforehand, and only translate new
            //segments (also change translation cache to account for different decoder configs for
            //same systems, i.e. keep track of decoder settings)

            FileInfo transAndAlign = new FileInfo($"{spOutput.FullName}.transandalign");
            var args = $"{this.modelDir.FullName} {spInput.FullName} {transAndAlign.FullName} --log-level=info --quiet";

            EventHandler exitHandler = (x, y) => BatchProcess_Exited(transAndAlign, spOutput, x, y);
            
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



        internal FileInfo PreprocessInput(IEnumerable<string> input, Boolean preprocessedInput=false)
        {
            var fileGuid = Guid.NewGuid();
            var srcFile = new FileInfo(Path.Combine(Path.GetTempPath(), $"{fileGuid}.{this.SourceCode}"));

            using (var srcStream = new StreamWriter(srcFile.FullName, true, Encoding.UTF8))
            {
                foreach (var line in input)
                {
                    srcStream.WriteLine(line);
                }
            }

            FileInfo spSrcFile;
            if (!preprocessedInput)
            {
                var spmModel = this.modelDir.GetFiles("source.spm").Single();
                spSrcFile = MarianHelper.PreprocessLanguage(srcFile, new DirectoryInfo(Path.GetTempPath()), this.SourceCode, spmModel, this.includePlaceholderTags, this.includeTagPairs);
            }
            else
            {
                spSrcFile = srcFile;
            }
            return spSrcFile;
        }

        private void errorDataHandler(object sender, DataReceivedEventArgs e)
        {
            Log.Information(e.Data);
        }
        
    }
    
}
    