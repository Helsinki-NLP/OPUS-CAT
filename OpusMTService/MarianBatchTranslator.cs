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

namespace FiskmoMTEngine
{
    public class MarianBatchTranslator
    {
        private string langpair;

        public string SourceCode { get; }
        public string TargetCode { get; }
        
        private DirectoryInfo modelDir;
        
        public string SystemName { get; }

        private bool includePlaceholderTags;
        private bool includeTagPairs;

        public MarianBatchTranslator(
            string modelDir, 
            string sourceCode, 
            string targetCode, 
            bool includePlaceholderTags, 
            bool includeTagPairs)
        {
            this.langpair = $"{sourceCode}-{targetCode}";
            this.SourceCode = sourceCode;
            this.TargetCode = targetCode;
            this.includePlaceholderTags = includePlaceholderTags;
            this.includeTagPairs = includeTagPairs;
            this.modelDir = new DirectoryInfo(modelDir);
            this.SystemName = $"{sourceCode}-{targetCode}_" + this.modelDir.Name;
            
            //Check if batch.yml exists, if not create it from decode.yml
            var batchYaml = this.modelDir.GetFiles("batch.yml");
            if (batchYaml.Length == 0)
            {
                var decoderYaml = this.modelDir.GetFiles("decoder.yml").Single();
                var deserializer = new Deserializer();
                var decoderSettings = deserializer.Deserialize<MarianDecoderConfig>(decoderYaml.OpenText());
                decoderSettings.miniBatch = "16";
                
                var serializer = new Serializer();
                var configPath = Path.Combine(this.modelDir.FullName, "batch.yml");
                using (var writer = File.CreateText(configPath))
                {
                    serializer.Serialize(writer, decoderSettings, typeof(MarianDecoderConfig));
                }
            }

        }

        internal Process BatchTranslate(List<string> input)
        {
            Log.Information($"Starting batch translator for model {this.SystemName}.");
            
            var cmd = "TranslateBatchSentencePiece.bat";
                        
            FileInfo spInput = this.PreprocessInput(input);
            FileInfo spOutput = new FileInfo(
                spInput.FullName.Replace($".{SourceCode}", $".{TargetCode}"));
            
            var args = $"{this.modelDir.FullName} {spInput.FullName} {spOutput.FullName}";
            var batchProcess = MarianHelper.StartProcessInBackgroundWithRedirects(cmd, args);

            batchProcess.Exited += (x,y)=> BatchProcess_Exited(input, spOutput,x,y);
            return batchProcess;
        }

        private void BatchProcess_Exited(List<string> input, FileInfo spOutput,object sender, EventArgs e)
        {
            
            Log.Information($"Batch translation process for model {this.SystemName} exited. Saving results.");
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

        private FileInfo PreprocessInput(List<string> input)
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

            var spmModel = this.modelDir.GetFiles("source.spm").Single();
            var spSrcFile = MarianHelper.PreprocessLanguage(srcFile, new DirectoryInfo(Path.GetTempPath()), this.SourceCode, spmModel, this.includePlaceholderTags, this.includeTagPairs);
            return spSrcFile;
        }

        private void errorDataHandler(object sender, DataReceivedEventArgs e)
        {
            Log.Information(e.Data);
        }
        
    }

}
    