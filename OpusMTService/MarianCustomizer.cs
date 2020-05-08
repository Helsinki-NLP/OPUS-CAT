using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using YamlDotNet.Serialization;

namespace FiskmoMTEngine
{

    class MarianCustomizer
    {
        private DirectoryInfo customDir;
        private DirectoryInfo modelDir;
        private FileInfo customSource;
        private FileInfo customTarget;
        private FileInfo validationSource;
        private FileInfo validationTarget;
        private string customLabel;
        private string sourceCode;
        private string targetCode;
        private FileInfo spSource;
        private FileInfo spTarget;
        private FileInfo spValidSource;
        private FileInfo spValidTarget;
        private MTModel selectedModel;
        
        private void CopyModelDir(DirectoryInfo modelDir,string customLabel)
        {
            this.customDir = new DirectoryInfo($"{modelDir.FullName}_{customLabel}");
            if (this.customDir.Exists)
            {
                throw new Exception("custom model directory exists already");
            }
            customDir.Create();

            foreach (FileInfo file in modelDir.GetFiles())
            {
                file.CopyTo(Path.Combine(customDir.FullName, file.Name));
            }
        }

        public void Customize()
        {
            //First copy the model to new dir
            try
            {
                this.CopyModelDir(this.modelDir, this.customLabel);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Customization failed: {ex.Message}", "Exception", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            //Preprocess input files
            this.PreprocessInput();

            var decoderYaml = this.customDir.GetFiles("decoder.yml").Single();
            var deserializer = new Deserializer();
            var decoderSettings = deserializer.Deserialize<MarianDecoderConfig>(decoderYaml.OpenText());

            MarianTrainerConfig trainingConfig;
            using (var reader = new StreamReader(OpusMTServiceSettings.Default.CustomizationBaseConfig))
            {
                trainingConfig = deserializer.Deserialize<MarianTrainerConfig>(reader);
            }
                
            trainingConfig.TrainSets = new List<string>
                    {
                        spSource.FullName,
                        spTarget.FullName
                    };

            trainingConfig.ValidSets = new List<string>
                    {
                        spValidSource.FullName,
                        spValidTarget.FullName
                    };

            trainingConfig.vocabs = new List<string>
                    {
                        Path.Combine(this.customDir.FullName, decoderSettings.vocabs[0]),
                        Path.Combine(this.customDir.FullName, decoderSettings.vocabs[0])
                    };

            trainingConfig.validLog = Path.Combine(this.customDir.FullName, "valid.log");

            trainingConfig.model = Path.Combine(this.customDir.FullName, decoderSettings.models.Single());

            var serializer = new Serializer();
            var configPath = Path.Combine(this.customDir.FullName, "customize.yml");
            using (var writer = File.CreateText(configPath))
            {
                serializer.Serialize(writer, trainingConfig, typeof(MarianTrainerConfig));
            }

            //var trainingArgs = $"--config {configPath} --log-level=warn";
            var trainingArgs = $"--config {configPath}";


            this.StartProcessWithCmd("marian.exe",trainingArgs);
        }

        private void PreprocessInput()
        {
            var sourceSpm = this.customDir.GetFiles("source.spm").Single();
            var targetSpm = this.customDir.GetFiles("target.spm").Single();

            this.spSource = this.PreprocessLanguage(this.customSource,this.sourceCode, sourceSpm);
            this.spTarget = this.PreprocessLanguage(this.customTarget, this.targetCode, targetSpm);

            this.spValidSource = this.PreprocessLanguage(this.validationSource, this.sourceCode, sourceSpm);
            this.spValidTarget = this.PreprocessLanguage(this.validationTarget, this.targetCode, targetSpm);
        }

        private FileInfo PreprocessLanguage(FileInfo languageFile, string languageCode, FileInfo spmModel)
        {
            var preprocessedFile = new FileInfo(Path.Combine(this.customDir.FullName, $"preprocessed_{languageFile.Name}"));
            var spFile = new FileInfo(Path.Combine(this.customDir.FullName, $"sp_{languageFile.Name}"));


            using (var rawFile = languageFile.OpenText())
            using (var preprocessedWriter = new StreamWriter(preprocessedFile.FullName))
            {
                String line;
                while ((line = rawFile.ReadLine()) != null)
                {
                    var preprocessedLine =
                        MosesPreprocessor.RunMosesPreprocessing(line, languageCode);
                    preprocessedLine = MosesPreprocessor.PreprocessSpaces(preprocessedLine);
                    preprocessedWriter.WriteLine(preprocessedLine);
                }
            }

            var spArgs = $"{preprocessedFile.FullName} --model {spmModel.FullName} --output {spFile.FullName}";
            this.StartProcessWithCmd("spm_encode.exe", spArgs);

            return spFile;
        }

        public MarianCustomizer(
            MTModel model,
            FileInfo customSource,
            FileInfo customTarget,
            FileInfo validationSource,
            FileInfo validationTarget,
            string customLabel)
        {
            this.modelDir = new DirectoryInfo(model.InstallDir);
            this.customSource = customSource;
            this.customTarget = customTarget;
            this.customLabel = customLabel;
            this.validationSource = validationSource;
            this.validationTarget = validationTarget;
            this.sourceCode = model.SourceLanguageString;
            this.targetCode = model.TargetLanguageString;
        }

        private void errorDataHandler(object sender, DataReceivedEventArgs e)
        {
            Log.Information(e.Data);
        }
        private Process StartProcessWithCmd(string fileName, string args)
        {
            var serviceDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            Process ExternalProcess = new Process();

            ExternalProcess.StartInfo.FileName = "cmd";
            ExternalProcess.StartInfo.Arguments = $"/c {fileName} {args}";
            ExternalProcess.StartInfo.UseShellExecute = false;
            //ExternalProcess.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;

            ExternalProcess.StartInfo.WorkingDirectory = serviceDir;
            //ExternalProcess.StartInfo.RedirectStandardInput = true;
            //ExternalProcess.StartInfo.RedirectStandardOutput = true;
            //ExternalProcess.StartInfo.RedirectStandardError = true;
            //ExternalProcess.StartInfo.StandardOutputEncoding = Encoding.UTF8;

            //ExternalProcess.ErrorDataReceived += errorDataHandler;

            ExternalProcess.StartInfo.CreateNoWindow = false;
            
            ExternalProcess.Start();
            //ExternalProcess.BeginErrorReadLine();

            //ExternalProcess.StandardInput.AutoFlush = true;

            return ExternalProcess;
        }
    }
}
