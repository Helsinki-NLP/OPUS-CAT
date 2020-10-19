using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

    public class MarianCustomizer
    {
        public event ProgressChangedEventHandler ProgressChanged;

        protected virtual void OnProgressChanged(ProgressChangedEventArgs e)
        {
            ProgressChangedEventHandler handler = ProgressChanged;
            handler?.Invoke(this, e);
        }

        public event EventHandler ProcessExited;

        protected virtual void OnProcessExited(EventArgs e)
        {
            EventHandler handler = ProcessExited;
            handler?.Invoke(this, e);
        }

        public DirectoryInfo customDir { get; set; }
        public MarianLog trainingLog = new MarianLog();

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
        private List<string> postCustomizationBatch;
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

        internal void MarianExitHandler(object sender, EventArgs e)
        {
            this.ProcessExited(sender, e);
        }

        //This parses Marian log file to detect finetuning progress
        internal void MarianProgressHandler(object sender, DataReceivedEventArgs e)
        {
            this.trainingLog.ParseTrainLogLine(e.Data);

            //Here convert the amount of processed lines / total lines into estimated progress
            //The progress start from five, so normalize it
            int newProgress;
            if (this.trainingLog.TotalLines > 0 && this.trainingLog.LinesSoFar > 0)
            {
                newProgress = 5 + Convert.ToInt32(0.95 * ((100 *this.trainingLog.LinesSoFar) / this.trainingLog.TotalLines));
            }
            else
            {
                newProgress = 5;
            }
            this.ProgressChanged(this, new ProgressChangedEventArgs(newProgress, new MarianCustomizationStatus(CustomizationStep.Customizing,this.trainingLog.EstimatedRemainingTotalTime)));
        }

        public enum CustomizationStep {
            Copying_model,
            Copying_training_files,
            Preprocessing_training_files,
            Initial_evaluation,
            Customizing };
        
        public Process Customize()
        {
            this.ProgressChanged(this, new ProgressChangedEventArgs(1, new MarianCustomizationStatus(CustomizationStep.Copying_model,null)));
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

            //Save the batch to translate after customization to a file (to be batch translated after succesful exit)
            if (this.postCustomizationBatch != null && this.postCustomizationBatch.Count > 0)
            {
                FileInfo postCustomizationBatchFile = new FileInfo(Path.Combine(this.customDir.FullName, FiskmoMTEngineSettings.Default.PostFinetuneBatchName));
                using (var writer = postCustomizationBatchFile.CreateText())
                {
                    foreach (var sourceString in this.postCustomizationBatch)
                    {
                        writer.WriteLine(sourceString);
                    }
                }
            }
            
            this.ProgressChanged(this, new ProgressChangedEventArgs(2, new MarianCustomizationStatus(CustomizationStep.Copying_training_files, null)));
            //Copy raw files to model dir
            this.customSource = this.customSource.CopyTo(Path.Combine(this.customDir.FullName, "custom.source"));
            this.customTarget= this.customTarget.CopyTo(Path.Combine(this.customDir.FullName, "custom.target"));

            this.ProgressChanged(this, new ProgressChangedEventArgs(3, new MarianCustomizationStatus(CustomizationStep.Preprocessing_training_files, null)));
            //Preprocess input files
            this.PreprocessInput();

            this.ProgressChanged(this, new ProgressChangedEventArgs(4, new MarianCustomizationStatus(CustomizationStep.Initial_evaluation, null)));
            //Do the initial evaluation
            var initialValidProcess = this.model.TranslateAndEvaluate(
                this.spValidSource,
                new FileInfo(Path.Combine(this.customDir.FullName, "valid.0.txt")),
                this.spValidTarget,
                FiskmoMTEngineSettings.Default.OODValidSetSize,
                true
                );

            //Wait for the initial valid to finish before starting customization
            //(TODO: make sure this is not done on UI thread)
            initialValidProcess.WaitForExit();

            this.ProgressChanged(this, new ProgressChangedEventArgs(5, new MarianCustomizationStatus(CustomizationStep.Customizing, null)));

            //Use the initial translation time as basis for estimating the duration of validation file
            //translation
            this.trainingLog.EstimatedTranslationDuration = Convert.ToInt32((initialValidProcess.ExitTime - initialValidProcess.StartTime).TotalSeconds);

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

            Process trainProcess = this.StartTraining();
            
            return trainProcess;
        }

        private Process StartTraining()
        {
            var configPath = Path.Combine(this.customDir.FullName, FiskmoMTEngineSettings.Default.CustomizationBaseConfig);

            var deserializer = new Deserializer();
            MarianTrainerConfig trainingConfig;
            using (var reader = new StreamReader(configPath))
            {
                trainingConfig = deserializer.Deserialize<MarianTrainerConfig>(reader);
            }

            this.trainingLog.TrainingConfig = trainingConfig;

            //var trainingArgs = $"--config {configPath} --log-level=warn";
            var trainingArgs = $"--config {configPath} --log-level=info"; // --quiet";

            var trainProcess = MarianHelper.StartProcessInBackgroundWithRedirects(
                Path.Combine(FiskmoMTEngineSettings.Default.MarianDir, "marian.exe"), trainingArgs, this.MarianExitHandler, this.MarianProgressHandler);
            return trainProcess;
        }

        internal Process ResumeCustomization()
        {
            var process = this.StartTraining();
            return process;
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
            DirectoryInfo customDir,
            List<string> postCustomizationBatch)
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

        public MarianCustomizer(DirectoryInfo customDir)
        {
            this.customDir = customDir;
        }
    }
}
