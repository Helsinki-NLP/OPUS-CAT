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
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using YamlDotNet.Serialization;

namespace OpusCatMtEngine
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
        private MTModel customModel;
        private DirectoryInfo modelDir;
        private FileInfo customSource;
        private FileInfo customTarget;
        private FileInfo inDomainValidationSource;
        private FileInfo inDomainValidationTarget;
        private string customLabel;
        private readonly bool includePlaceholderTags;
        private readonly bool includeTagPairs;
        private IsoLanguage sourceLanguage;
        private IsoLanguage targetLanguage;
        private bool guidedAlignment;
        private List<string> postCustomizationBatch;
        private string segmentationMethod;
        private FileInfo spSource;
        private FileInfo spTarget;
        private FileInfo spValidSource;
        private FileInfo spValidTarget;
        private FileInfo alignmentFile;
        private FileInfo validAlignmentFile;
        private bool performFinalEvaluation;

        private void CopyModelDir(DirectoryInfo modelDir,string customLabel)
        {
            if (this.customDir.Exists)
            {

                //throw new Exception("custom model directory exists already");
            }

            this.customDir.Create();

            //TODO: this should not copy everything
            foreach (FileInfo file in modelDir.GetFiles())
            {
                file.CopyTo(Path.Combine(this.customDir.FullName, file.Name));
            }
        }

        internal void MarianExitHandler(object sender, EventArgs e)
        {
            //Separate final evaluation is disabled, since Marian will in any case perform
            //validation in the end (at least if epoch limit is reached, not sure about other
            //end conditions).
            this.performFinalEvaluation = false;
            if (this.performFinalEvaluation)
            {
                var finalValidProcess = this.customModel.TranslateAndEvaluate(
                    new FileInfo(this.trainingLog.TrainingConfig.ValidSets[0]),
                    new FileInfo(Path.Combine(this.customDir.FullName, "valid.final.txt")),
                    new FileInfo(this.trainingLog.TrainingConfig.ValidSets[1]),
                    OpusCatMtEngineSettings.Default.OODValidSetSize,
                    this.sourceLanguage,
                    this.targetLanguage,
                    true
                    );
                finalValidProcess.WaitForExit();
            }
            
            this.OnProcessExited(e);
        }


        //This parses Marian log file to detect finetuning progress
        internal void MarianProgressHandler(object sender, DataReceivedEventArgs e)
        {
            //Data will be null in when the process exits
            if (e.Data != null)
            {
                //If there is no marian log file yet, log the output to opuscat log
                var marianLog = new FileInfo(Path.Combine(this.customDir.FullName, "train.log"));
                if (!marianLog.Exists)
                {
                    Log.Information(e.Data);
                }
                this.trainingLog.ParseTrainLogLine(e.Data);
                //Check here for Marian error, if it happens trigger ui to show customization as suspended.
                if (this.trainingLog.EncounteredError)
                {
                    this.OnProgressChanged(new ProgressChangedEventArgs(0, new MarianCustomizationStatus(CustomizationStep.Error, this.trainingLog.EstimatedRemainingTotalTime)));
                    return;
                }

                //Here convert the amount of processed lines / total lines into estimated progress
                //The progress start from five, so normalize it
                int newProgress;
                if (this.trainingLog.TotalLines > 0 && this.trainingLog.SentencesSoFar > 0)
                {
                    newProgress = 5 + Convert.ToInt32(0.95 * ((100 * this.trainingLog.SentencesSoFar) / (this.trainingLog.TotalLines)));
                }
                else
                {
                    newProgress = 5;
                }
                this.OnProgressChanged(new ProgressChangedEventArgs(newProgress, new MarianCustomizationStatus(CustomizationStep.Finetuning, this.trainingLog.EstimatedRemainingTotalTime)));
            }
            
        }

        public enum CustomizationStep {
            Copying_model,
            Copying_training_files,
            Preprocessing_training_files,
            Initial_evaluation,
            Finetuning,
            Error
        };
        
        public Process Customize()
        {
            this.OnProgressChanged(new ProgressChangedEventArgs(1, new MarianCustomizationStatus(CustomizationStep.Copying_model,null)));
            //First copy the model to new dir
            try
            {
                this.CopyModelDir(this.modelDir, this.customLabel);
                //Save model config as soon as the model dir exists
                this.customModel.SaveModelConfig();
            }
            catch (Exception ex)
            {
                Log.Information($"Customization failed: {ex.Message}");
                return null;
            }

            //Save the batch to translate after customization to a file (to be batch translated after successful exit)
            if (this.postCustomizationBatch != null && this.postCustomizationBatch.Count > 0)
            {
                FileInfo postCustomizationBatchFile = new FileInfo(Path.Combine(this.customDir.FullName, OpusCatMtEngineSettings.Default.PostFinetuneBatchName));
                using (var writer = postCustomizationBatchFile.CreateText())
                {
                    foreach (var sourceString in this.postCustomizationBatch)
                    {
                        writer.WriteLine(sourceString);
                    }
                }
            }
            
            this.OnProgressChanged(new ProgressChangedEventArgs(2, new MarianCustomizationStatus(CustomizationStep.Copying_training_files, null)));
            //Copy raw files to model dir
            this.customSource = this.customSource.CopyTo(Path.Combine(this.customDir.FullName, "custom.source"));
            this.customTarget= this.customTarget.CopyTo(Path.Combine(this.customDir.FullName, "custom.target"));

            this.OnProgressChanged(new ProgressChangedEventArgs(3, new MarianCustomizationStatus(CustomizationStep.Preprocessing_training_files, null)));
            //Preprocess input files
            this.PreprocessInput();

            var decoderYaml = this.customDir.GetFiles("decoder.yml").Single();
            var deserializer = new Deserializer();

            var decoderSettings = deserializer.Deserialize<MarianDecoderConfig>(decoderYaml.OpenText());

            // TODO: this is meant for generating alignments for fine-tuning models with guided alignment.
            // However, it seems that it's not necessary to provide alignments for fine-tuning 
            // (at least the effects are not huge), so this has not been implemented yet. Also, this 
            // uses eflomal, which is GPL and so difficult to integrate. I'll leave this code here,
            // but I'll remove the actual eflomal components, they can be reintroduced if needed.
            if (this.guidedAlignment)
            {
                //Generate alignments for fine-tuning corpus
                //this.alignmentFile = new FileInfo(Path.Combine(this.customDir.FullName, "custom.alignments"));
                //MarianHelper.GenerateAlignments(this.spSource, this.spTarget, this.alignmentFile, this.model.AlignmentPriorsFile);

                //
                //Generate alignments for validation set (for evaluating fine-tuning effect on alignment)
                //this.validAlignmentFile = new FileInfo(Path.Combine(this.customDir.FullName, "combined.alignments"));
                //MarianHelper.GenerateAlignments(this.spValidSource, this.spValidTarget, this.validAlignmentFile, this.model.AlignmentPriorsFile);
            }

            this.OnProgressChanged(new ProgressChangedEventArgs(4, new MarianCustomizationStatus(CustomizationStep.Initial_evaluation, null)));
            //Do the initial evaluation
            var initialValidProcess = this.model.TranslateAndEvaluate(
                this.spValidSource,
                new FileInfo(Path.Combine(this.customDir.FullName, "valid.0.txt")),
                this.spValidTarget,
                OpusCatMtEngineSettings.Default.OODValidSetSize,
                this.sourceLanguage,
                this.targetLanguage,
                true
                );

            //Wait for the initial valid to finish before starting customization
            //(TODO: make sure this is not done on UI thread)
            initialValidProcess.WaitForExit();
            
            this.OnProgressChanged(new ProgressChangedEventArgs(6, new MarianCustomizationStatus(CustomizationStep.Finetuning, null)));

            //Use the initial translation time as basis for estimating the duration of validation file
            //translation
            this.trainingLog.EstimatedTranslationDuration = Convert.ToInt32((initialValidProcess.ExitTime - initialValidProcess.StartTime).TotalSeconds);
            
            MarianTrainerConfig trainingConfig;
            
            var baseCustomizeYmlPath =
                HelperFunctions.GetLocalAppDataPath(
                    OpusCatMtEngineSettings.Default.CustomizationBaseConfig);

            var processDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            //Make sure there's a customization file.
            if (!File.Exists(baseCustomizeYmlPath))
            {
                
                File.Copy(
                    Path.Combine(processDir,OpusCatMtEngineSettings.Default.CustomizationBaseConfig), 
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

            /* This is the old method, using a batch file, now uses python script
             * switch (this.segmentationMethod)
            {

                case ".bpe":
                    string validScriptPath = Path.Combine(this.customDir.FullName, "ValidateBpe.bat");
                    trainingConfig.validScriptPath = 
                        $"\"{validScriptPath}\"";
                    File.Copy(
                        Path.Combine(processDir, "ValidateBpe.bat"), validScriptPath);
                    break;
                case ".spm":
                    validScriptPath = Path.Combine(this.customDir.FullName, "ValidateSp.bat");
                    trainingConfig.validScriptPath =
                        $"\"{validScriptPath}\"";
                    File.Copy(
                        Path.Combine(processDir, "ValidateSp.bat"), validScriptPath);
                    break;
                default:
                    break;
            }*/

            trainingConfig.validScriptPath =
                Path.Combine(Path.Combine(processDir, OpusCatMtEngineSettings.Default.PythonDir), "python.exe .\\Marian\\validate.py");

            trainingConfig.validScriptArgs = 
                new List<string> {
                    $"{spValidTarget.FullName}",
                    OpusCatMtEngineSettings.Default.OODValidSetSize.ToString(),
                    this.segmentationMethod};
            trainingConfig.validTranslationOutput = Path.Combine(this.customDir.FullName,"valid.{U}.txt");

            if (this.guidedAlignment)
            {
                trainingConfig.guidedAlignment = this.alignmentFile.FullName;
            }

            trainingConfig.validLog = Path.Combine(this.customDir.FullName, "valid.log");
            trainingConfig.log = Path.Combine(this.customDir.FullName, "train.log");

            trainingConfig.model = Path.Combine(this.customDir.FullName, decoderSettings.models.Single());
            
            var builder = new SerializerBuilder();
            builder.ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull);
            var serializer = builder.Build();
            
            var configPath = Path.Combine(this.customDir.FullName, OpusCatMtEngineSettings.Default.CustomizationBaseConfig);
            using (var writer = File.CreateText(configPath))
            {
                serializer.Serialize(writer, trainingConfig, typeof(MarianTrainerConfig));
            }

            Process trainProcess = this.StartTraining();
            
            return trainProcess;
        }



        private Process StartTraining()
        {
            var configPath = Path.Combine(this.customDir.FullName, OpusCatMtEngineSettings.Default.CustomizationBaseConfig);

            var deserializer = new Deserializer();
            MarianTrainerConfig trainingConfig;
            using (var reader = new StreamReader(configPath))
            {
                trainingConfig = deserializer.Deserialize<MarianTrainerConfig>(reader);
            }

            this.trainingLog.TrainingConfig = trainingConfig;

            //var trainingArgs = $"--config {configPath} --log-level=warn";
            var trainingArgs = $"--config \"{configPath}\" --log-level=info"; // --quiet";

            /*byte[] bytes = Encoding.Default.GetBytes(trainingArgs);
            trainingArgs = Encoding.UTF8.GetString(bytes);*/

            // Fine-tuning tends to cause IO lockup when the model is written, so assign low priority
            // by changing main process priority to 
            var parent = Process.GetCurrentProcess();
            var original = parent.PriorityClass;

            parent.PriorityClass = ProcessPriorityClass.BelowNormal;

            var trainProcess = MarianHelper.StartProcessInBackgroundWithRedirects(
                Path.Combine(OpusCatMtEngineSettings.Default.MarianDir, "marian.exe"), trainingArgs, this.MarianExitHandler, this.MarianProgressHandler);

            //Restore normal process priority
            parent.PriorityClass = original;


            return trainProcess;
        }

        internal Process ResumeCustomization(MTModel customModel)
        {
            this.customModel = customModel;
            var process = this.StartTraining();
            return process;
        }

        private void PreprocessInput()
        {
            FileInfo sourceSegModel = 
                this.customDir.GetFiles().Where(x => Regex.IsMatch(x.Name, "source.(spm|bpe)")).Single();
            FileInfo targetSegModel =
                this.customDir.GetFiles().Where(x => Regex.IsMatch(x.Name, "target.(spm|bpe)")).Single();

            this.segmentationMethod = sourceSegModel.Extension;

            var targetPrefix = this.model.TargetLanguages.Count > 1 ? this.targetLanguage.OriginalCode : null;
            this.spSource = MarianHelper.PreprocessLanguage(
                this.customSource,
                this.customDir,
                this.sourceLanguage.OriginalCode,
                sourceSegModel,
                this.includePlaceholderTags,
                this.includeTagPairs,
                targetPrefix);
            this.spTarget = MarianHelper.PreprocessLanguage(
                this.customTarget, 
                this.customDir, 
                this.targetLanguage.OriginalCode, 
                targetSegModel, 
                this.includePlaceholderTags, 
                this.includeTagPairs);

            //concatenate the out-of-domain validation set with the in-domain validation set
            ParallelFilePair tatoebaValidFileInfos = 
                HelperFunctions.GetTatoebaFileInfos(this.sourceLanguage.ShortestIsoCode, this.targetLanguage.ShortestIsoCode);
            if (tatoebaValidFileInfos == null)
            {
                tatoebaValidFileInfos = HelperFunctions.GenerateDummyOODValidSet(this.customDir);
            }

            ParallelFilePair combinedValid = new ParallelFilePair(
                tatoebaValidFileInfos,
                new ParallelFilePair(this.inDomainValidationSource, this.inDomainValidationTarget),
                this.customDir.FullName,
                OpusCatMtEngineSettings.Default.OODValidSetSize);

            this.spValidSource = MarianHelper.PreprocessLanguage(
                combinedValid.Source, 
                this.customDir, 
                this.sourceLanguage.OriginalCode, 
                sourceSegModel, 
                this.includePlaceholderTags, 
                this.includeTagPairs,
                targetPrefix);
            this.spValidTarget = MarianHelper.PreprocessLanguage(
                combinedValid.Target, 
                this.customDir, 
                this.targetLanguage.OriginalCode, 
                targetSegModel, 
                this.includePlaceholderTags, 
                this.includeTagPairs);
        }
        
        public MarianCustomizer(
            MTModel model,
            MTModel customModel,
            ParallelFilePair inputPair,
            ParallelFilePair indomainValidPair,
            string customLabel,
            bool includePlaceholderTags,
            bool includeTagPairs,
            List<string> postCustomizationBatch,
            IsoLanguage sourceLanguage,
            IsoLanguage targetLanguage,
            bool guidedAlignment=false)
        {
            this.model = model;
            this.customModel = customModel;
            this.modelDir = new DirectoryInfo(model.InstallDir);
            this.customDir = new DirectoryInfo(this.customModel.InstallDir);
            this.customSource = inputPair.Source;
            this.customTarget = inputPair.Target;
            this.customLabel = customLabel;
            this.includePlaceholderTags = includePlaceholderTags;
            this.includeTagPairs = includeTagPairs;
            this.inDomainValidationSource = indomainValidPair.Source;
            this.inDomainValidationTarget = indomainValidPair.Target;
            this.sourceLanguage = sourceLanguage;
            this.targetLanguage = targetLanguage;
            this.guidedAlignment = guidedAlignment;
        }

        public MarianCustomizer(DirectoryInfo customDir)
        {
            this.customDir = customDir;
        }
    }
}
