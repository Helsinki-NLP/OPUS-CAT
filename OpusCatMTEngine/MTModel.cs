using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity.Migrations.History;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using YamlDotNet.Serialization;

namespace OpusCatMTEngine
{
    
    public enum MTModelStatus
    {
        OK,
        Finetuning,
        Finetuning_suspended,
        Preprocessing_failed
    }

    public class EnumToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string EnumString;
            try
            {
                EnumString = Enum.GetName((value.GetType()), value);
                return Regex.Replace(EnumString,"_"," ");
            }
            catch
            {
                return string.Empty;
            }
        }

        // No need to implement converting back on a one-way binding 
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public enum TagMethod
    {
        Remove,
        IncludePlaceholders
    }

    

    public class MTModel : INotifyPropertyChanged
    {
        public object this[string propertyName]
        {
            get
            {
                // probably faster without reflection:
                // like:  return Properties.Settings.Default.PropertyValues[propertyName] 
                // instead of the following
                Type myType = typeof(MTModel);
                PropertyInfo myPropInfo = myType.GetProperty(propertyName);
                return myPropInfo.GetValue(this, null);
            }
            set
            {
                Type myType = typeof(MTModel);
                PropertyInfo myPropInfo = myType.GetProperty(propertyName);
                myPropInfo.SetValue(this, value, null);
            }

        }

        private List<IsoLanguage> sourceLanguages;
        private List<IsoLanguage> targetLanguages;
        private string name;

        private Boolean isOverrideModel;
        public bool IsOverrideModel
        {
            get => isOverrideModel;
            set
            {
                isOverrideModel = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged("IsNotOverrideModel");
            }
        }

        public bool IsNotOverrideModel { get => !this.isOverrideModel; }

        private Boolean isOverridden;

        public bool IsOverridden
        {
            get => isOverridden;
            set
            {
                isOverridden = value;
                NotifyPropertyChanged();
            }
        }

        public FileInfo AlignmentPriorsFile {
            get { return new FileInfo(Path.Combine(this.InstallDir, "alignmentpriors.txt")); } }

        private MarianProcess marianProcess;

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        internal void Shutdown()
        {
            if (this.marianProcess != null)
            {
                this.marianProcess.ShutdownMtPipe();
                this.marianProcess = null;
            }
        }

        internal Task<string> Translate(string input)
        {
            if (this.marianProcess == null)
            {
                this.marianProcess = new MarianProcess(this.InstallDir, this.SourceLanguageString, this.TargetLanguageString, this.modelConfig.IncludePlaceholderTags, this.modelConfig.IncludeTagPairs);
            }

            return this.marianProcess.AddToTranslationQueue(input);
        }

        internal void DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            this.InstallProgress = e.ProgressPercentage;
        }

        public List<IsoLanguage> TargetLanguages { get => targetLanguages; set => targetLanguages = value; }

        public MTModelStatus Status { get => status; set { status = value; NotifyPropertyChanged(); } }

        //This creates a zip package of the model that can be moved to another computer
        internal void PackageModel()
        {
            var customModelZipDirPath = HelperFunctions.GetLocalAppDataPath(OpusCatMTEngineSettings.Default.CustomModelZipPath);
            if (!Directory.Exists(customModelZipDirPath))
            {
                Directory.CreateDirectory(customModelZipDirPath);
            }

            var zipPath = Path.Combine(customModelZipDirPath, $"{this.Name}.zip");

            if (File.Exists(zipPath))
            {
                MessageBox.Show($"Zipped model already exists: {zipPath}");
                return;
            }

            using (var packageZip = ZipFile.Open(zipPath, ZipArchiveMode.Create))
            {

                //Include model files, README.md, spm files, tcmodel (not needed but expected), modelconfig.yml
                var decoderYaml = new DirectoryInfo(this.InstallDir).GetFiles("decoder.yml").Single();
                var deserializer = new Deserializer();
                var decoderSettings = deserializer.Deserialize<MarianDecoderConfig>(decoderYaml.OpenText());

                foreach (var modelNpzName in decoderSettings.models)
                {
                    //No point in compressing npz, it's already compressed
                    packageZip.CreateEntryFromFile(Path.Combine(this.InstallDir, modelNpzName), modelNpzName, CompressionLevel.NoCompression);
                }

                foreach (var vocabName in decoderSettings.vocabs.Distinct())
                {
                    packageZip.CreateEntryFromFile(Path.Combine(this.InstallDir, vocabName), vocabName);
                }

                var otherModelFiles =
                    new List<string>()
                    {
                        "source.spm",
                        "target.spm",
                        "source.tcmodel",
                        "decoder.yml",
                        "README.md",
                        "preprocess.sh",
                        "postprocess.sh",
                        "modelconfig.yml",
                        "LICENSE"
                    };

                foreach (var fileName in otherModelFiles)
                {
                    packageZip.CreateEntryFromFile(Path.Combine(this.InstallDir, fileName), fileName);
                }
            }
            
        }

        public string StatusAndEstimateString
        {
            get
            {
                string statusAndEstimate;
                if (this.CustomizationStatus != null)
                {
                    if (this.CustomizationStatus.CustomizationStep == MarianCustomizer.CustomizationStep.Error)
                    {
                        return "Error during fine-tuning\nCheck fine-tuning log for details";
                    }

                    statusAndEstimate = HelperFunctions.EnumToString(this.CustomizationStatus.CustomizationStep);
                    if (!(this.CustomizationStatus.EstimatedSecondsRemaining == null))
                    {
                        var estimatedTimeSpan = TimeSpan.FromSeconds(
                                this.CustomizationStatus.EstimatedSecondsRemaining.Value);

                        if (this.CustomizationStatus.EstimatedSecondsRemaining.Value == 0)
                        {
                            statusAndEstimate = $"{statusAndEstimate}\nWaiting for time estimate";
                        }
                        else
                        {
                            statusAndEstimate = $"{statusAndEstimate}\nEstimated time remaining {estimatedTimeSpan.Hours} h {estimatedTimeSpan.Minutes} min";
                        }
                        
        
                    }
                }
                else
                {
                    statusAndEstimate = this.Status.ToString();
                }

                return statusAndEstimate;
            }
        }

        internal void ResumeTraining()
        {
            var customizer = new MarianCustomizer(new DirectoryInfo(this.InstallDir));
            customizer.ProgressChanged += this.CustomizationProgressHandler;
            customizer.ProcessExited += this.ExitHandler;
            this.FinetuneProcess = customizer.ResumeCustomization(this);

            this.Status = MTModelStatus.Finetuning;

            NotifyPropertyChanged("IsCustomizationSuspended");
        }

        internal void ExitHandler(object sender, EventArgs e)
        {
            if (this.IsCustomizationSuspended.Value)
            {
                this.Status = MTModelStatus.Finetuning_suspended;
            }
            else
            {
                this.Status = MTModelStatus.OK;
                this.ModelConfig.FinetuningComplete = true;
                this.SaveModelConfig();
                this.CustomizationStatus = null;
                this.StatusProgress = 0;
                this.NotifyPropertyChanged("StatusAndEstimateString");
                FileInfo postCustomizationBatchFile = new FileInfo(Path.Combine(this.InstallDir, OpusCatMTEngineSettings.Default.PostFinetuneBatchName));
                if (postCustomizationBatchFile.Exists)
                {
                    List<string> newSegments = new List<string>();
                    using (var reader = postCustomizationBatchFile.OpenText())
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            newSegments.Add(line);
                        }
                    }

                    //Fine-tuned models are always bilingual models
                    this.PreTranslateBatch(newSegments,this.sourceLanguages.First(), this.targetLanguages.First());
                }
            }
        }

        public List<IsoLanguage> SourceLanguages { get => sourceLanguages; set => sourceLanguages = value; }

        public string Name { get => name; set => name = value; }

        public int InstallProgress { get => installProgress; set { installProgress = value; NotifyPropertyChanged(); } }
        private int installProgress = 0;

        public string InstallStatus { get => installStatus; set { installStatus = value; NotifyPropertyChanged(); } }
        private string installStatus = "";
        private bool _prioritized;

        public int StatusProgress { get => statusProgress; set { statusProgress = value; NotifyPropertyChanged(); } }

        public MarianCustomizationStatus CustomizationStatus { get; private set; }

        private int statusProgress = 0;

        void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            this.InstallProgress = e.ProgressPercentage;
        }

        public MTModel(string modelPath, string installDir)
        {
            this.InstallDir = installDir;
            this.ParseModelPath(modelPath);

            var modelConfigPath = Path.Combine(this.InstallDir, "modelconfig.yml");
            if (File.Exists(modelConfigPath))
            {
                var deserializer = new Deserializer();
                using (var reader = new StreamReader(modelConfigPath))
                {
                    this.ModelConfig = deserializer.Deserialize<MTModelConfig>(reader);
                }

                //Check if the model finetuning has been suspended (because the MT engine has been closed,
                //or there's been an error)
                if (this.ModelConfig.FinetuningInitiated && !this.ModelConfig.FinetuningComplete)
                {
                    if (this.IsCustomizationSuspended.Value)
                    {
                        FileInfo trainingLog = new FileInfo(
                        Path.Combine(
                            this.InstallDir,
                            OpusCatMTEngineSettings.Default.TrainLogName));

                        if (trainingLog.Exists)
                        {
                            this.Status = MTModelStatus.Finetuning_suspended;
                        }
                        else
                        {
                            this.Status = MTModelStatus.Preprocessing_failed;
                        }
                    }
                }
            }
            else
            {
                this.ModelConfig = new MTModelConfig();
                this.SaveModelConfig();
            }

            this.ModelConfig.ModelTags.CollectionChanged += ModelTags_CollectionChanged;
        }

        internal void SaveModelConfig()
        {
            //The directory might not exists yet in case of customized models (i.e. copying of the base model
            //is not complete)
            if (Directory.Exists(this.InstallDir))
            {
                var modelConfigPath = Path.Combine(this.InstallDir, "modelconfig.yml");
                var serializer = new Serializer();
                using (var writer = File.CreateText(modelConfigPath))
                {
                    serializer.Serialize(writer, this.ModelConfig, typeof(MTModelConfig));
                }
            }
        }

        internal Process TranslateAndEvaluate(
            FileInfo sourceFile, 
            FileInfo targetFile, 
            FileInfo refFile,
            int outOfDomainSize,
            Boolean preprocessedInput=false)
        {
            var batchTranslator = new MarianBatchTranslator(
                this.InstallDir,
                this.SourceLanguages.Single(),
                this.TargetLanguages.Single(), false, false);

            var sourceLines = File.ReadAllLines(sourceFile.FullName);
            batchTranslator.OutputReady += (x,y) => EvaluateTranslation(refFile, outOfDomainSize, targetFile);
            var batchProcess = batchTranslator.BatchTranslate(sourceLines, targetFile, preprocessedInput);
            
            return batchProcess;
        }

        private void EvaluateTranslation(
            FileInfo refFile, 
            int outOfDomainSize, 
            FileInfo spOutput)
        {
            var evalProcess = MarianHelper.StartProcessInBackgroundWithRedirects(
                "Validate.bat",
                $"{refFile.FullName} {outOfDomainSize} {spOutput.FullName}");
        }

        private void ModelTags_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            this.SaveModelConfig();
        }
        
        private void ParseModelPath(string modelPath)
        {
            char separator;
            if (modelPath.Contains('/'))
            {
                separator = '/';
            }
            else
            {
                separator = '\\';
            }
            var pathSplit = modelPath.Split(separator);
            //Multiple source languages separated by plus symbols
            this.SourceLanguages = pathSplit[0].Split('-')[0].Split('+').Select(x => new IsoLanguage(x)).ToList();
            this.TargetLanguages = pathSplit[0].Split('-')[1].Split('+').Select(x => new IsoLanguage(x)).ToList();
            this.Name = pathSplit[1];
            this.ModelPath = modelPath;
        }

        //This is mainly for creating finetuned models, hence the process argument (which holds the fine tuning process)
        public MTModel(
            string name, 
            string modelPath, 
            List<IsoLanguage> sourceLangs,
            List<IsoLanguage> targetLangs,
            MTModelStatus status, 
            string modelTag, 
            DirectoryInfo customDir,
            Process finetuneProcess,
            bool includePlaceholderTags,
            bool includeTagPairs)
        {
            this.Name = name;
            this.SourceLanguages = sourceLangs;
            this.TargetLanguages = targetLangs;
            this.Status = status;
            this.FinetuneProcess = finetuneProcess;
            this.ModelConfig = new MTModelConfig();
            this.ModelConfig.ModelTags.Add(modelTag);
            this.ModelConfig.IncludePlaceholderTags = includePlaceholderTags;
            this.ModelConfig.IncludeTagPairs = includeTagPairs;

            if (status == MTModelStatus.Finetuning)
            {
                this.ModelConfig.FinetuningInitiated = true;
                this.TrainingLog = new MarianLog();
            }
            this.InstallDir = customDir.FullName;
            this.ModelPath = modelPath;
            this.SaveModelConfig();
        }

        public MTModel(string modelPath)
        {
            this.ParseModelPath(modelPath);
        }
    

        public Boolean IsReady
        {
            get
            {
                if (this.IsCustomizationSuspended == null)
                {
                    return true;
                }
                else
                {
                    return !this.IsCustomizationSuspended.Value;
                }
            }
        }

        public Boolean IsCustomizationFinished
        {
            get
            {
                return (this.IsCustomizationSuspended.HasValue && !this.IsCustomizationSuspended.Value);
            }
        }

        //Indicates whether customization has been suspended, null value is for noncustomized models
        public Boolean? IsCustomizationSuspended
        {
            get
            {
                if (!this.ModelConfig.FinetuningInitiated)
                {
                    return null;
                }

                if (this.FinetuneProcess == null || this.FinetuneProcess.HasExited)
                {
                    //Parse train.log to determine whether training is done
                    FileInfo trainingLog = new FileInfo(
                        Path.Combine(
                            this.InstallDir,
                            OpusCatMTEngineSettings.Default.TrainLogName));

                    if (trainingLog.Exists)
                    {
                        try
                        {
                            using (var reader = trainingLog.OpenText())
                            {
                                string line;
                                while ((line = reader.ReadLine()) != null)
                                {
                                    if (line.EndsWith("Training finished"))
                                    {
                                        this.ModelConfig.FinetuningComplete = true;
                                        this.SaveModelConfig();
                                        return false;
                                    }
                                }
                            }
                            return true;
                        }
                        catch (IOException ex)
                        {
                            Log.Information($"Train.log for customized system {this.Name} has been locked by another process. The previous training process is probably still running in the background (wait for it to finish or end it). Exception: {ex.Message}");
                            return true;
                        }
                    }
                    else
                    {
                        //If there's no process and no train.log, this indicates that customization
                        //has failed before the training has started. This means the finetuning process
                        //is not salvageable

                        return true;
                    }
                }
                else
                {
                    return false;
                }
            }
        }

        public string SourceLanguageString
        {
            get { return String.Join("+", this.SourceLanguages); }
        }

        public string TargetLanguageString
        {
            get { return String.Join("+", this.TargetLanguages); }
        }

        public string ModelPath { get; internal set; }
        public string InstallDir { get; }
        public bool Prioritized { get => _prioritized; set { _prioritized = value; NotifyPropertyChanged(); } }

        private MarianLog TrainingLog;

        public MTModelConfig ModelConfig { get => modelConfig; set => modelConfig = value; }
        public Process FinetuneProcess { get; set; }
        

        private MTModelStatus status;
        private MTModelConfig modelConfig;
        private IsoLanguage srcLang;
        private IsoLanguage trgLang;
        private MTModelStatus customizing;
        private string modelTag;
        
        private object includePlaceholderTags;
        private object includeTagPairs;

        internal Process PreTranslateBatch(List<string> input, IsoLanguage sourceLang, IsoLanguage targetLang)
        {
            FileInfo output = new FileInfo(Path.Combine(this.InstallDir, "batchoutput.txt"));
            var batchTranslator = new MarianBatchTranslator(
                this.InstallDir, 
                sourceLang, 
                targetLang, 
                this.modelConfig.IncludePlaceholderTags,
                this.modelConfig.IncludeTagPairs);
            return batchTranslator.BatchTranslate(input,output,storeTranslations:true);
        }

        internal void CustomizationProgressHandler(object sender, ProgressChangedEventArgs e)
        {
            this.StatusProgress = e.ProgressPercentage;
            this.CustomizationStatus = (MarianCustomizationStatus)e.UserState;
            this.NotifyPropertyChanged("StatusAndEstimateString");
        }
    }
}
