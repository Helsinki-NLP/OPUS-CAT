using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity.Migrations.History;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace FiskmoMTEngine
{
    
    public enum MTModelStatus
    {
        OK,
        Customizing
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
        
        public List<string> SourceLanguages { get => sourceLanguages; set => sourceLanguages = value; }

        public string Name { get => name; set => name = value; }

        public int InstallProgress { get => installProgress; set { installProgress = value; NotifyPropertyChanged(); } }
        private int installProgress = 0;

        public string InstallStatus { get => installStatus; set { installStatus = value; NotifyPropertyChanged(); } }
        private string installStatus = "";
        private bool _prioritized;


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
            }
            else
            {
                this.ModelConfig = new MTModelConfig();
                this.SaveModelConfig();
            }

            this.ModelConfig.ModelTags.CollectionChanged += ModelTags_CollectionChanged;
        }

        private void SaveModelConfig()
        {
            var modelConfigPath = Path.Combine(this.InstallDir, "modelconfig.yml");
            var serializer = new Serializer();
            using (var writer = File.CreateText(modelConfigPath))
            {
                serializer.Serialize(writer, this.ModelConfig, typeof(MTModelConfig));
            }
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

        public MTModel(string name, string modelPath, string sourceCode, string targetCode, MTModelStatus status, string modelTag)
        {
            this.Name = name;
            this.SourceLanguages = new List<string>() { sourceCode };
            this.TargetLanguages = new List<string>() { targetCode };
            this.Status = status;
            this.ModelConfig = new MTModelConfig();
            this.ModelConfig.ModelTags.Add(modelTag);
            this.ModelPath = modelPath;

        }

        public MTModel(string modelPath)
        {
            this.ParseModelPath(modelPath);
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

        public MTModelConfig ModelConfig { get => modelConfig; set => modelConfig = value; }

        private MTModelStatus status;
        private MTModelConfig modelConfig;

        internal void PreTranslateBatch(List<string> input)
        {
            var batchProcess = new MarianBatchTranslator(this.InstallDir, this.SourceLanguageString, this.TargetLanguageString, this.modelConfig.IncludePlaceholderTags,this.modelConfig.IncludeTagPairs);
            batchProcess.BatchTranslate(input);
        }
    }
}
