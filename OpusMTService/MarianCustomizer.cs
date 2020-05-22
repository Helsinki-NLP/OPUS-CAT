using Serilog;
using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations.History;
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
        public DirectoryInfo customDir { get; set; }
        private DirectoryInfo modelDir;
        private FileInfo customSource;
        private FileInfo customTarget;
        private FileInfo validationSource;
        private FileInfo validationTarget;
        private string customLabel;
        private string sourceCode;
        private string targetCode;
        private TagMethod tagMethod;
        private FileInfo spSource;
        private FileInfo spTarget;
        private FileInfo spValidSource;
        private FileInfo spValidTarget;
        private MTModel selectedModel;
        
        private void CopyModelDir(DirectoryInfo modelDir,string customLabel)
        {
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

        public void Customize(EventHandler exitHandler)
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
            using (var reader = new StreamReader(FiskmoMTEngineSettings.Default.CustomizationBaseConfig))
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


            var trainProcess = MarianHelper.StartProcessInWindow("marian.exe",trainingArgs);

            if (exitHandler != null)
            {
                trainProcess.Exited += exitHandler;
            }
        }

        private void PreprocessInput()
        {
            var sourceSpm = this.customDir.GetFiles("source.spm").Single();
            var targetSpm = this.customDir.GetFiles("target.spm").Single();

            this.spSource = MarianHelper.PreprocessLanguage(this.customSource, this.customDir, this.sourceCode, sourceSpm, this.tagMethod);
            this.spTarget = MarianHelper.PreprocessLanguage(this.customTarget, this.customDir, this.targetCode, targetSpm, this.tagMethod);

            this.spValidSource = MarianHelper.PreprocessLanguage(this.validationSource, this.customDir, this.sourceCode, sourceSpm, this.tagMethod);
            this.spValidTarget = MarianHelper.PreprocessLanguage(this.validationTarget, this.customDir, this.targetCode, targetSpm, this.tagMethod);
        }


        public MarianCustomizer(
            MTModel model,
            FileInfo customSource,
            FileInfo customTarget,
            FileInfo validationSource,
            FileInfo validationTarget,
            string customLabel,
            TagMethod tagMethod)
        {
            this.modelDir = new DirectoryInfo(model.InstallDir);
            this.customDir = new DirectoryInfo($"{modelDir.FullName}_{customLabel}");
            this.customSource = customSource;
            this.customTarget = customTarget;
            this.customLabel = customLabel;
            this.validationSource = validationSource;
            this.validationTarget = validationTarget;
            this.sourceCode = model.SourceLanguageString;
            this.targetCode = model.TargetLanguageString;
            this.tagMethod= tagMethod;
        }

    }
}
