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

    public enum SegmentationMethod
    {
        Bpe,
        SentencePiece
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

        public string TatoebaConfigString
        {
            get
            { return this.modelYaml; }
        }

        public bool IsMultilingualModel
        {
            get
            { return this.SourceLanguages.Count > 1 || this.TargetLanguages.Count > 1; }
        }

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

        private bool supportsWordAlignment;

        public bool CanSetAsOverrideModel
        {
            get 
            {
                return this.Status == MTModelStatus.OK && this.IsNotOverrideModel;
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
        
        private Dictionary<Tuple<string,string>,MarianProcess> marianProcesses;

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
            if (this.marianProcesses != null)
            {
                foreach (var langpair in this.marianProcesses.Keys)
                {
                    this.marianProcesses[langpair].ShutdownMtPipe();
                    this.marianProcesses[langpair] = null;
                }
            }
            this.marianProcesses = null;
        }

        internal Task<TranslationPair> Translate(string input, IsoLanguage sourceLang, IsoLanguage targetLang)
        {
            //Need to get the original codes, since those are the ones the marian model uses
            var modelOrigSourceCode =
                this.SourceLanguages.Single(x => x.ShortestIsoCode == sourceLang.ShortestIsoCode).OriginalCode;
            var modelOrigTargetCode =
                this.TargetLanguages.Single(x => x.ShortestIsoCode == targetLang.ShortestIsoCode).OriginalCode;

            var modelOrigTuple = new Tuple<string, string>(modelOrigSourceCode, modelOrigTargetCode);
            
            //TODO: Currently a new process is created for each language direction of a multilingual model.
            //I don't see much concurrent use of multilingual model language pairs happening, but it would be
            //nice if all requests could be directed to same model.
            if (this.marianProcesses == null)
            {
                this.marianProcesses = new Dictionary<Tuple<string, string>, MarianProcess>();
            }

            if (!this.marianProcesses.ContainsKey(modelOrigTuple))
            {
                 this.marianProcesses[modelOrigTuple] =
                    new MarianProcess(
                        this.InstallDir, 
                        modelOrigSourceCode,
                        modelOrigTargetCode,
                        $"{this.SourceCodesString}-{this.TargetCodesString}_{this.Name}",
                        this.targetLanguages.Count > 1,
                        this.modelConfig.IncludePlaceholderTags, this.modelConfig.IncludeTagPairs);
            };

            return this.marianProcesses[modelOrigTuple].AddToTranslationQueue(input);
        }

        internal void DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            this.InstallProgress = e.ProgressPercentage;
        }

        public List<IsoLanguage> TargetLanguages
        {
            get => targetLanguages;
            set => targetLanguages = value;
        }

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
                foreach (var modelNpzName in this.decoderSettings.models)
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
                    switch (this.Status)
                    {
                        case MTModelStatus.Preprocessing_failed:
                            statusAndEstimate = "Fine-tuning failed.\nDelete the model.";
                            break;
                        case MTModelStatus.Finetuning_suspended:
                            statusAndEstimate = "Fine-tuning suspended.";
                            break;
                        default:
                            statusAndEstimate = this.Status.ToString();
                            break;
                    }
                    
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
            this.CheckFinetuningState();
            if (this.Status == MTModelStatus.OK)
            {
                this.SaveModelConfig();
                this.CustomizationStatus = null;
                this.StatusProgress = 0;
                //Update all properties by passing empty string
                this.NotifyPropertyChanged(String.Empty);
                //this.NotifyPropertyChanged("StatusAndEstimateString");
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

        private void CheckFinetuningState()
        {
            //Check for train.log to determine whether training has been started/is finished
            this.trainingLogFileInfo = new FileInfo(
                Path.Combine(
                    this.InstallDir,
                    OpusCatMTEngineSettings.Default.TrainLogName));

            //If training log does not exist, Marian training has not been started.
            //This is currently an unrecoverable error.
            this.trainingLogFileInfo.Refresh();
            if (!trainingLogFileInfo.Exists)
            {
                this.Status = MTModelStatus.Preprocessing_failed;
            }
            else
            {
                //Training log exists, so training has started, check if it has finished
                try
                {
                    using (var reader = this.trainingLogFileInfo.OpenText())
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            if (line.EndsWith("Training finished"))
                            {
                                this.Status = MTModelStatus.OK;
                                break;
                            }
                            else
                            {
                                this.Status = MTModelStatus.Finetuning_suspended;
                            }
                        }
                    }
                }
                catch (IOException ex)
                {
                    Log.Information($"Train.log for customized system {this.Name} has been locked by another process. The previous training process is probably still running in the background (wait for it to finish or end it). Exception: {ex.Message}");
                    this.Status = MTModelStatus.Finetuning;
                }
            }
        }

        public bool CanContinueCustomization
        {
            get { return this.Status == MTModelStatus.Finetuning_suspended; }
        }

        public DateTime ModelDate
        {
            get
            {
                var DateString = Regex.Match(this.ModelPath, @"\d{4}-\d{2}-\d{2}$");
                if (DateString.Success)
                {
                    return DateTime.Parse(DateString.Value);
                }
                else
                {
                    return DateTime.MinValue;
                }
            }
        }

        //Name with language pair but without date
        public string ModelBaseName
        {
            get
            {
                var datelessName = Regex.Replace(this.ModelPath, @"-\d{4}-\d{2}-\d{2}$","");
                return datelessName;
            }
        }

        private void ParseDecoderConfig()
        {
            var decoderYaml = new DirectoryInfo(this.InstallDir).GetFiles("decoder.yml").Single();
            var deserializer = new Deserializer();
            this.decoderSettings = deserializer.Deserialize<MarianDecoderConfig>(decoderYaml.OpenText());
            
        }

        private void ParseModelConfig()
        {
            var deserializer = new Deserializer();
            var modelConfigPath = Path.Combine(this.InstallDir, "modelconfig.yml");
            if (File.Exists(modelConfigPath))
            {
                using (var reader = new StreamReader(modelConfigPath))
                {
                    this.ModelConfig = deserializer.Deserialize<MTModelConfig>(reader);
                }

                //Check the state of a fine-tuned model
                if (this.ModelConfig.Finetuned)
                {
                    this.CheckFinetuningState();
                }
            }
            else
            {
                this.ModelConfig = new MTModelConfig();
                this.SaveModelConfig();
            }
        }

        private void UpdateModelYamlPath()
        {
            //Recent models have yaml files containing metadata, they have the same name as the model npz file
            var modelFilePath = Path.Combine(this.InstallDir, this.decoderSettings.models[0]);
            this.modelYamlFilePath = Path.ChangeExtension(modelFilePath, "yml");
        }

        public MTModel(string modelPath, string installDir)
        {
            this.InstallDir = installDir;

            this.ParseDecoderConfig();
            this.UpdateModelYamlPath();

            this.SupportsWordAlignment = this.decoderSettings.models[0].Contains("-align");

            if (File.Exists(this.modelYamlFilePath))
            {
                using (var reader = File.OpenText(this.modelYamlFilePath))
                {
                    this.modelYaml = reader.ReadToEnd();
                } 
            }

            this.ParseModelPath(modelPath);

            this.ParseModelConfig();
            
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
                this.TargetLanguages.Single(),
                this.ModelSegmentationMethod,
                false,
                false);

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

            string validateScript;

            switch (this.ModelSegmentationMethod)
            {
                case SegmentationMethod.Bpe:
                    validateScript = "ValidateBpe.bat";
                    break;
                case SegmentationMethod.SentencePiece:
                    validateScript = "ValidateSp.bat";
                    break;
                default:
                    return;
            }

            var evalProcess = MarianHelper.StartProcessInBackgroundWithRedirects(
                validateScript,
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

            //For OPUS-MT models and monolingual Tatoeba models, 
            //languages are included in path. For multilingual Tatoeba models,
            //languages have to be fetched the metadata yml file
            if (this.modelYaml == null)
            {
                this.SourceLanguages =
                    pathSplit[0].Split('-')[0].Split('+').Select(x => new IsoLanguage(x)).ToList();
                this.TargetLanguages =
                    pathSplit[0].Split('-')[1].Split('+').Select(x => new IsoLanguage(x)).ToList();
            }
            else
            {
                this.ParseModelYaml(this.modelYaml);
            }
            
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
            this.InstallDir = customDir.FullName;

            if (status == MTModelStatus.Finetuning)
            {
                this.ModelConfig.Finetuned = true;
                this.TrainingLog = new MarianLog();
                this.trainingLogFileInfo = new FileInfo(
                Path.Combine(
                    this.InstallDir,
                    OpusCatMTEngineSettings.Default.TrainLogName));
            }
            
            this.ModelPath = modelPath;
            this.SaveModelConfig();
        }

        //This is used for online models, model uri is included for later download of models
        public MTModel(string modelPath, Uri modelUri, string yamlString=null)
        {
            this.modelYaml = yamlString;
            this.ModelUri = modelUri;
            this.ParseModelPath(modelPath);
        }

        private void ParseModelYaml(string yamlString)
        {

            try
            {
                this.SourceLanguages = new List<IsoLanguage>();
                this.TargetLanguages = new List<IsoLanguage>();

                var deserializer = new DeserializerBuilder().Build();
                var res = deserializer.Deserialize<dynamic>(yamlString);

                List<object> xamlSourceLangs = null;
                if (res.ContainsKey("source-languages"))
                {
                    xamlSourceLangs = res["source-languages"];
                }
                
                if (xamlSourceLangs != null)
                {
                    foreach (var lang in xamlSourceLangs)
                    {
                        this.SourceLanguages.Add(new IsoLanguage(lang.ToString()));
                    }
                }
                else
                {
                    Log.Error($"No source langs in {this.ModelUri} yaml file.");
                    this.Faulted = true;
                    this.SourceLanguages.Add(new IsoLanguage("NO SOURCE LANGUAGES"));
                }
                List<object> xamlTargetLangs = null;
                if (res.ContainsKey("target-languages"))
                {
                    xamlTargetLangs = res["target-languages"];
                }

                if (xamlTargetLangs != null)
                {
                    foreach (var lang in xamlTargetLangs)
                    {
                        this.TargetLanguages.Add(new IsoLanguage(lang.ToString()));
                    }
                }
                else
                {
                    Log.Error($"No target langs in {this.ModelUri} yaml file.");
                    this.Faulted = true;
                    this.TargetLanguages.Add(new IsoLanguage("NO TARGET LANGUAGES"));
                }
            }
            catch (YamlDotNet.Core.SyntaxErrorException ex)
            {
                Log.Error($"Error in the yaml syntax of model {this.ModelUri}. Error: {ex.Message}.");
                this.SourceLanguages.Add(new IsoLanguage("ERROR IN YAML SYNTAX"));
                this.TargetLanguages.Add(new IsoLanguage("ERROR IN YAML SYNTAX"));
            }
            catch (Exception ex)
            {
                Log.Error($"source-langs or target-langs key missing from {this.ModelUri} yaml file.");
            }
            
        }

        //This indicates whether the model is ready for translation
        //(essentially means whether fine-tuning has finished if the model is a fine-tuned model).
        public Boolean IsReady
        {
            get
            {
                return this.Status == MTModelStatus.OK;
            }
        }

        public String ModelOrigin
        {
            get
            {
                return this.ModelUri.Segments[1];
                
            }
        }

        public Boolean CanContinueFinetuning
        {
            get
            {
                return this.Status == MTModelStatus.Finetuning_suspended;
            }
        }

        public Boolean HasProgress
        {
            get
            {
                if (this.trainingLogFileInfo != null)
                {
                    this.trainingLogFileInfo.Refresh();
                    return this.trainingLogFileInfo.Exists;
                }
                else
                {
                    return false;
                } 
            }
        }

        public Boolean CanPackage
        {
            get
            {
                if (this.trainingLogFileInfo != null)
                {
                    this.trainingLogFileInfo.Refresh();
                    return this.Status == MTModelStatus.OK;
                }
                else
                {
                    return false;
                }
                
            }
        }

        public Boolean CanDelete
        {
            get
            {
                return this.Status != MTModelStatus.Finetuning;
            }
        }

        public Boolean CanTranslate
        {
            get
            {
                return this.Status == MTModelStatus.OK;
            }
        }

        public string SourceLanguageString
        {
            get { return String.Join(", ", this.SourceLanguages.Select(x => x.IsoRefName)); }
        }

        public string SourceCodesString
        {
            get { return String.Join("+", this.SourceLanguages); }
        }

        public string TargetLanguageString
        {
            get { return String.Join(", ", this.TargetLanguages.Select(x => x.IsoRefName)); }
        }

        public string TargetCodesString
        {
            get { return String.Join("+", this.TargetLanguages); }
        }

        public string ModelPath { get; internal set; }
        public string InstallDir { get; }
        public bool Prioritized { get => _prioritized; set { _prioritized = value; NotifyPropertyChanged(); } }
        
        private MarianLog TrainingLog;

        public MTModelConfig ModelConfig { get => modelConfig; set => modelConfig = value; }
        public Process FinetuneProcess { get; set; }

        private string modelYaml;

        public Uri ModelUri { get; private set; }
        public bool Faulted { get; private set; }

        private MarianDecoderConfig decoderSettings;

        public bool SupportsWordAlignment { get => supportsWordAlignment; set => supportsWordAlignment = value; }
        public bool DoesNotSupportWordAlignment { get => !supportsWordAlignment; }
        public SegmentationMethod ModelSegmentationMethod
        {
            get
            {
                if (Directory.GetFiles(this.InstallDir).Any(x => x.EndsWith("source.bpe")))
                {
                    return SegmentationMethod.Bpe;
                }
                else
                {
                    return SegmentationMethod.SentencePiece;
                }
            }
        }

        private MTModelStatus status;
        private MTModelConfig modelConfig;
        private FileInfo trainingLogFileInfo;
        private string modelYamlFilePath;

        internal Process PreTranslateBatch(List<string> input, IsoLanguage sourceLang, IsoLanguage targetLang)
        {
            FileInfo output = new FileInfo(Path.Combine(this.InstallDir, "batchoutput.txt"));
            var batchTranslator = new MarianBatchTranslator(
                this.InstallDir, 
                sourceLang, 
                targetLang, 
                this.ModelSegmentationMethod,
                this.modelConfig.IncludePlaceholderTags,
                this.modelConfig.IncludeTagPairs);
            return batchTranslator.BatchTranslate(input,output,storeTranslations:true);
        }

        internal void CustomizationProgressHandler(object sender, ProgressChangedEventArgs e)
        {
            this.StatusProgress = e.ProgressPercentage;
            this.CustomizationStatus = (MarianCustomizationStatus)e.UserState;
            //TODO: Why does this not update the UI?
            this.NotifyPropertyChanged("HasProgress");
            this.NotifyPropertyChanged("StatusAndEstimateString");
        }
    }
}
