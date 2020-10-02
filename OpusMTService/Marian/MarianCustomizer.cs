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

        private MTModel model;
        private DirectoryInfo modelDir;
        private FileInfo customSource;
        private FileInfo customTarget;
        private FileInfo inDomainValidationSource;
        private FileInfo inDomainValidationTarget;
        private string customLabel;
        private readonly bool includePlaceholderTags;
        private readonly bool includeTagPairs;
        private string sourceCode;
        private string targetCode;
        private FileInfo spSource;
        private FileInfo spTarget;
        private FileInfo spValidSource;
        private FileInfo spValidTarget;
        
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

        public Process Customize(EventHandler exitHandler=null, DataReceivedEventHandler errorDataHandler=null)
        {
            //First copy the model to new dir
            try
            {
                this.CopyModelDir(this.modelDir, this.customLabel);
            }
            catch (Exception ex)
            {
                Log.Information($"Customization failed: {ex.Message}");
                return null;
            }

            //Copy raw files to model dir
            this.customSource = this.customSource.CopyTo(Path.Combine(this.customDir.FullName, "custom.source"));
            this.customTarget= this.customTarget.CopyTo(Path.Combine(this.customDir.FullName, "custom.target"));

            //Preprocess input files
            this.PreprocessInput();

            //Do the initial evaluation
            var initialValidProcess = this.model.TranslateAndEvaluate(
                this.spValidSource,
                new FileInfo(Path.Combine(this.customDir.FullName, "valid.0.txt")),
                this.spValidTarget,
                new FileInfo(Path.Combine(this.customDir.FullName, "valid.0.txt")),
                FiskmoMTEngineSettings.Default.OODValidSetSize,
                true
                );

            //Wait for the initial valid to finish before starting customization
            initialValidProcess.WaitForExit();

            var decoderYaml = this.customDir.GetFiles("decoder.yml").Single();
            var deserializer = new Deserializer();
            
            var decoderSettings = deserializer.Deserialize<MarianDecoderConfig>(decoderYaml.OpenText());

            MarianTrainerConfig trainingConfig;

            
            var baseCustomizeYmlPath =
                HelperFunctions.GetLocalAppDataPath(
                    FiskmoMTEngineSettings.Default.CustomizationBaseConfig);

            var processDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            //Make sure there's a customization file.
            if (!File.Exists(baseCustomizeYmlPath))
            {
                
                File.Copy(
                    Path.Combine(processDir,FiskmoMTEngineSettings.Default.CustomizationBaseConfig), 
                    baseCustomizeYmlPath);
            }

            //deserialize yaml file
            using (var reader = new StreamReader(baseCustomizeYmlPath))
            {
                trainingConfig = deserializer.Deserialize<MarianTrainerConfig>(reader);
            }
                
            trainingConfig.trainSets = new List<string>
                    {
                        this.spSource.FullName,
                        this.spTarget.FullName
                    };

            trainingConfig.ValidSets = new List<string>
                    {
                        this.spValidSource.FullName,
                        this.spValidTarget.FullName
                    };

            trainingConfig.vocabs = new List<string>
                    {
                        Path.Combine(this.customDir.FullName, decoderSettings.vocabs[0]),
                        Path.Combine(this.customDir.FullName, decoderSettings.vocabs[0])
                    };

            
            trainingConfig.validScriptPath = Path.Combine(this.customDir.FullName, "Validate.bat");
            File.Copy(
                Path.Combine(processDir,"Validate.bat"), trainingConfig.validScriptPath);

            trainingConfig.validScriptArgs = 
                new List<string> { spValidTarget.FullName, FiskmoMTEngineSettings.Default.OODValidSetSize.ToString()};
            trainingConfig.validTranslationOutput = Path.Combine(this.customDir.FullName,"valid.{U}.txt");

            trainingConfig.validLog = Path.Combine(this.customDir.FullName, "valid.log");
            trainingConfig.log = Path.Combine(this.customDir.FullName, "train.log");

            trainingConfig.model = Path.Combine(this.customDir.FullName, decoderSettings.models.Single());

            var builder = new SerializerBuilder();
            builder.ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull);
            var serializer = builder.Build();
            
            var configPath = Path.Combine(this.customDir.FullName, FiskmoMTEngineSettings.Default.CustomizationBaseConfig);
            using (var writer = File.CreateText(configPath))
            {
                serializer.Serialize(writer, trainingConfig, typeof(MarianTrainerConfig));
            }

            //var trainingArgs = $"--config {configPath} --log-level=warn";
            var trainingArgs = $"--config {configPath} --log-level=info"; // --quiet";
            
            var trainProcess = MarianHelper.StartProcessInBackgroundWithRedirects(
                Path.Combine(FiskmoMTEngineSettings.Default.MarianDir,"marian.exe"),trainingArgs,exitHandler,errorDataHandler);

            return trainProcess;
        }

        private void PreprocessInput()
        {
            var sourceSpm = this.customDir.GetFiles("source.spm").Single();
            var targetSpm = this.customDir.GetFiles("target.spm").Single();

            this.spSource = MarianHelper.PreprocessLanguage(this.customSource, this.customDir, this.sourceCode, sourceSpm, this.includePlaceholderTags,this.includeTagPairs);
            this.spTarget = MarianHelper.PreprocessLanguage(this.customTarget, this.customDir, this.targetCode, targetSpm, this.includePlaceholderTags, this.includeTagPairs);

            //concatenate the out-of-domain validation set with the in-domain validation set
            ParallelFilePair tatoebaValidFileInfos = HelperFunctions.GetTatoebaFileInfos(this.sourceCode, this.targetCode);
            ParallelFilePair combinedValid = new ParallelFilePair(
                tatoebaValidFileInfos,
                new ParallelFilePair(this.inDomainValidationSource, this.inDomainValidationTarget),
                this.customDir.FullName,
                FiskmoMTEngineSettings.Default.OODValidSetSize);

            this.spValidSource = MarianHelper.PreprocessLanguage(combinedValid.Source, this.customDir, this.sourceCode, sourceSpm, this.includePlaceholderTags, this.includeTagPairs);
            this.spValidTarget = MarianHelper.PreprocessLanguage(combinedValid.Target, this.customDir, this.targetCode, targetSpm, this.includePlaceholderTags, this.includeTagPairs);
        }


        public MarianCustomizer(
            MTModel model,
            ParallelFilePair inputPair,
            ParallelFilePair indomainValidPair,
            string customLabel,
            bool includePlaceholderTags,
            bool includeTagPairs,
            DirectoryInfo customDir)
        {
            this.model = model;
            this.modelDir = new DirectoryInfo(model.InstallDir);
            this.customDir = customDir;
            this.customSource = inputPair.Source;
            this.customTarget = inputPair.Target;
            this.customLabel = customLabel;
            this.includePlaceholderTags = includePlaceholderTags;
            this.includeTagPairs = includeTagPairs;
            this.inDomainValidationSource = indomainValidPair.Source;
            this.inDomainValidationTarget = indomainValidPair.Target;
            this.sourceCode = model.SourceLanguageString;
            this.targetCode = model.TargetLanguageString;
        }

    }
}
