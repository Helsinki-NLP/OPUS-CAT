using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity.Migrations.History;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Data;
using YamlDotNet.Serialization;

namespace FiskmoMTEngine
{
    
    public enum MTModelStatus
    {
        OK,
        Customizing,
        Customization_suspended
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
        private List<string> sourceLanguages;
        private List<string> targetLanguages;
        private string name;

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

        internal string Translate(string input)
        {
            if (this.marianProcess == null)
            {
                this.marianProcess = new MarianProcess(this.InstallDir, this.SourceLanguageString, this.TargetLanguageString, this.modelConfig.IncludePlaceholderTags, this.modelConfig.IncludeTagPairs);
            }

            return this.marianProcess.Translate(input);
        }

        internal void DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            this.InstallProgress = e.ProgressPercentage;
        }

        public List<string> TargetLanguages { get => targetLanguages; set => targetLanguages = value; }

        public MTModelStatus Status { get => status; set { status = value; NotifyPropertyChanged(); } }

        internal void ResumeTraining()
        {
            FileInfo customizationConfig =
                new FileInfo(Path.Combine(
                    this.InstallDir,
                    FiskmoMTEngineSettings.Default.CustomizationBaseConfig));
            var trainingArgs = $"--config {customizationConfig.FullName} --log-level=info --quiet";

            var trainProcess = MarianHelper.StartProcessInBackgroundWithRedirects(
                Path.Combine(FiskmoMTEngineSettings.Default.MarianDir, "marian.exe"),
                trainingArgs,
                exitHandler);

            this.FinetuneProcess = trainProcess;

            this.Status = MTModelStatus.Customizing;

            NotifyPropertyChanged("IsCustomizationSuspended");
        }

        private void exitHandler(object sender, EventArgs e)
        {
            if (this.IsCustomizationSuspended.Value)
            {
                this.Status = MTModelStatus.Customization_suspended;
            }
            else
            {
                this.Status = MTModelStatus.OK;
            }
        }

        public List<string> SourceLanguages { get => sourceLanguages; set => sourceLanguages = value; }

        public string Name { get => name; set => name = value; }

        public int InstallProgress { get => installProgress; set { installProgress = value; NotifyPropertyChanged(); } }
        private int installProgress = 0;

        public string InstallStatus { get => installStatus; set { installStatus = value; NotifyPropertyChanged(); } }
        private string installStatus = "";
        private bool _prioritized;

        public int CustomizationProgress { get => customizationProgress; set { customizationProgress = value; NotifyPropertyChanged(); } }
        private int customizationProgress = 0;

        public string CustomizationStatus { get => customizationStatus; set { customizationStatus = value; NotifyPropertyChanged(); } }
        private string customizationStatus = "";
        

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
                if (this.ModelConfig.Finetuned)
                {
                    if (this.IsCustomizationSuspended.Value)
                    {
                        this.Status = MTModelStatus.Customization_suspended;
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
            FileInfo scoreFile,
            int outOfDomainSize,
            Boolean preprocessedInput=false)
        {
            var batchTranslator = new MarianBatchTranslator(
                this.InstallDir,
                this.SourceLanguages.Single(),
                this.TargetLanguages.Single(), false, false);

            var sourceLines = File.ReadAllLines(sourceFile.FullName);
            var batchProcess = batchTranslator.BatchTranslate(sourceLines, targetFile, (x) => EvaluateTranslation(refFile, outOfDomainSize, x),preprocessedInput);
            
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
            this.SourceLanguages = pathSplit[0].Split('-')[0].Split('+').ToList();
            this.TargetLanguages = pathSplit[0].Split('-')[1].Split('+').ToList();
            this.Name = pathSplit[1];
            this.ModelPath = modelPath;
        }

        //This is mainly for creating finetuned models, hence the process argument (which holds the fine tuning process)
        public MTModel(
            string name, 
            string modelPath, 
            string sourceCode, 
            string targetCode, 
            MTModelStatus status, 
            string modelTag, 
            DirectoryInfo customDir,
            Process finetuneProcess)
        {
            this.Name = name;
            this.SourceLanguages = new List<string>() { sourceCode };
            this.TargetLanguages = new List<string>() { targetCode };
            this.Status = status;
            this.FinetuneProcess = finetuneProcess;
            this.ModelConfig = new MTModelConfig();
            this.ModelConfig.ModelTags.Add(modelTag);
            if (status == MTModelStatus.Customizing)
            {
                this.ModelConfig.Finetuned = true;
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

        //Indicates whether customization has been suspended, null value is for noncustomized models
        public Boolean? IsCustomizationSuspended
        {
            get
            {
                if (!this.ModelConfig.Finetuned)
                {
                    return null;
                }

                if (this.FinetuneProcess == null || this.FinetuneProcess.HasExited)
                {
                    //Parse train.log to determine whether training is done
                    FileInfo trainingLog = new FileInfo(
                        Path.Combine(
                            this.InstallDir,
                            FiskmoMTEngineSettings.Default.TrainLogName));

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
                                        return false;
                                    }
                                }
                            }
                            return true;
                        }
                        catch (IOException ex)
                        {
                            Log.Information($"Train.log for customized system {this.Name} has been locked by another process. The previous training process is probably stil running in the background (wait for it to finish or end it). Exception: {ex.Message}");
                            return true;
                        }
                    }
                    else
                    {
                        //If there's no process and no train.log, this indicates that customization
                        //has failed before the training has started
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

        internal Process PreTranslateBatch(List<string> input)
        {
            FileInfo output = new FileInfo(Path.Combine(this.InstallDir, "batchoutput.txt"));
            var batchTranslator = new MarianBatchTranslator(
                this.InstallDir, 
                this.SourceLanguageString, 
                this.TargetLanguageString, 
                this.modelConfig.IncludePlaceholderTags,
                this.modelConfig.IncludeTagPairs);
            return batchTranslator.BatchTranslate(input,output);
        }

        internal void CustomizationProgressHandler(object sender, DataReceivedEventArgs e)
        {
            this.TrainingLog.ParseTrainLogLine(e.Data);
        }

        

    }
}
