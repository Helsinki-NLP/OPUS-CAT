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
                decoderSettings.log = Path.Combine(this.modelDir.FullName,"batch.log");

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
            Action<FileInfo> callBack=null,
            Boolean preprocessedInput=false)
        {
            Log.Information($"Starting batch translator for model {this.SystemName}.");
            
            var cmd = "TranslateBatchSentencePiece.bat";
                        
            FileInfo spInput = this.PreprocessInput(input,preprocessedInput);
            
            //TODO: check the translation cache for translations beforehand, and only translate new
            //segments (also change translation cache to account for different decoder configs for
            //same systems, i.e. keep track of decoder settings)

            var args = $"{this.modelDir.FullName} {spInput.FullName} {spOutput.FullName} --log-level=info --quiet";

            EventHandler exitHandler;
            if (callBack != null)
            {
                exitHandler = (x, y) => callBack(spOutput);
            }
            else
            {
                //default callback, saves translation to translation cache
                exitHandler = (x, y) => BatchProcess_Exited(input, spOutput, x, y);
            }
            var batchProcess = MarianHelper.StartProcessInBackgroundWithRedirects(cmd, args, exitHandler);
            

            return batchProcess;
        }

        private void BatchProcess_Exited(IEnumerable<string> input, FileInfo spOutput,object sender, EventArgs e)
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

    internal class MarianBatchArgs : EventArgs
    {
        internal IEnumerable<string> Input;
        internal FileInfo SpOutput;
    }
}
    