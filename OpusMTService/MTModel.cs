using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
                this.marianProcess = new MarianProcess(this.InstallDir, this.SourceLanguageString, this.TargetLanguageString);
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


            var modelTagFilePath = Path.Combine(this.InstallDir, "modeltags.txt");
            if (File.Exists(modelTagFilePath))
            {
                using (var sr = new StreamReader(modelTagFilePath))
                {
                    while (!sr.EndOfStream)
                    {
                        this.ModelTags.Add(sr.ReadLine());
                    }
                }
            }

            this.ModelTags.CollectionChanged += ModelTags_CollectionChanged;
        }

        private void ModelTags_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            var modelTagFilePath = Path.Combine(this.InstallDir, "modeltags.txt");
            using (var sw = new StreamWriter(modelTagFilePath))
            {
                foreach (var tag in this.ModelTags)
                {
                    sw.WriteLine(tag);
                }
            }
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

        public MTModel(string name, string sourceCode, string targetCode, MTModelStatus status)
        {
            this.Name = name;
            this.SourceLanguages = new List<string>() { sourceCode };
            this.TargetLanguages = new List<string>() { targetCode };
            this.Status = status;
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

        public ObservableCollection<string> ModelTags = new ObservableCollection<string>();
        private bool customizationOngoing;
        private MTModelStatus status;

        internal void PreTranslateBatch(List<string> input)
        {
            var batchProcess = new MarianBatchTranslator(this.InstallDir, this.SourceLanguageString, this.TargetLanguageString);
            batchProcess.BatchTranslate(input);
        }
    }
}
