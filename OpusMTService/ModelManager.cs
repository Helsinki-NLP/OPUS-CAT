using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using static System.Environment;

namespace OpusMTService
{
    /// <summary>
    /// This class contains methods for checking and downloading latest models from
    /// the fiskmo model repository. 
    /// </summary>
    public class ModelManager
    {

        private ObservableCollection<ModelInfo> latestModelInfo;

        public ObservableCollection<ModelInfo> LatestModelInfo
        { get => latestModelInfo; set { latestModelInfo = value; NotifyPropertyChanged(); } }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        
        private DirectoryInfo fiskmoAppdataDir;
        private string downloadPath;



        public DirectoryInfo FiskmoAppdataDir { get => fiskmoAppdataDir; }

        public ModelManager()
        {
            this.LatestModelInfo = this.GetLatestModelInfo();
            this.fiskmoAppdataDir = new DirectoryInfo(OpusMTServiceSettings.Default.LocalFiskmoDir);
            if (!FiskmoAppdataDir.Exists)
            {
                FiskmoAppdataDir.Create();
            }
            
        }

        private ObservableCollection<ModelInfo> GetLatestModelInfo()
        {
            try
            {
                WebRequest request = WebRequest.Create(OpusMTServiceSettings.Default.ModelStorageUrl);
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream dataStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream);
                string responseFromServer = reader.ReadToEnd();
                XDocument buckets = XDocument.Parse(responseFromServer);
                var ns = buckets.Root.Name.Namespace;
                var files = buckets.Descendants(ns + "Key").Select(x => x.Value);
                var models = files.Where(x => x.StartsWith("models") && x.EndsWith(".zip"));
                return new ObservableCollection<ModelInfo>(models.Select(x => new ModelInfo(x)));
            }
            catch
            {
                return null;
            }
        }

        internal string[] GetAllModelDirs(string sourceLang, string targetLang)
        {
            if (Directory.Exists(
                Path.Combine(this.fiskmoAppdataDir.FullName, "models", $"{sourceLang}-{targetLang}")))
            {
                var languagePairModels = Directory.GetDirectories(Path.Combine(
                    this.fiskmoAppdataDir.FullName, "models", $"{sourceLang}-{targetLang}"));
                return languagePairModels;
            }
            else
            {
                return null;
            }
        }

        internal IEnumerable<string> GetAllLanguagePairs()
        {
            //The format of the model strings is "models/<source>-target/<name>"
            return this.LatestModelInfo.Select(x => x.Name.Split('/')[1]);
        }

        internal string GetLatestModelDir(string sourceLang, string targetLang)
        {
            var allModels = this.GetAllModelDirs(sourceLang, targetLang);
            if (allModels == null)
            {
                return null;
            }
            else
            {
                var newestLanguagePairModel = allModels.OrderBy(x => Regex.Match(x, @"\d{8}").Value).LastOrDefault();
                return newestLanguagePairModel;
            }
        }

        internal void DownloadModel(
            string newerModel, 
            DownloadProgressChangedEventHandler wc_DownloadProgressChanged,
            AsyncCompletedEventHandler wc_DownloadComplete)
        {
            this.downloadPath = Path.Combine(this.FiskmoAppdataDir.FullName, newerModel);

            using (var client = new WebClient())
            {
                client.DownloadProgressChanged += wc_DownloadProgressChanged;
                client.DownloadFileCompleted += wc_DownloadComplete;
                var modelUrl = $"{OpusMTServiceSettings.Default.ModelStorageUrl}/{newerModel}";
                Directory.CreateDirectory(Path.GetDirectoryName(downloadPath));
                client.DownloadFileAsync(new Uri(modelUrl), downloadPath);
            }
            
        }

        internal void ExtractModel()
        {
            string extractionFolder = Regex.Replace(downloadPath, @"\.zip$", "");
            ZipFile.ExtractToDirectory(this.downloadPath, extractionFolder);
            File.Delete(this.downloadPath);
        }


        public Boolean IsLanguagePairSupported(string sourceLang, string targetLang)
        {
            //This method is called many times (possibly for each segment), so use the cached model list instead of
            //new model fetch call to check whether pair is supported
            return this.LatestModelInfo.Any(x => x.Name.Contains($"{sourceLang}-{targetLang}"));
        }

        //Check for existence of current Fiskmo models in ProgramData
        public string CheckForNewerModel(string sourceLang, string targetLang)
        {
            string newerModel = null;
            //var timestamps = this.GetLatestModelInfo().Select(x => Regex.Match(x, @"\d{8}").Value);
            //order models by timestamp
            var onlineModels = this.LatestModelInfo.OrderBy(x => Regex.Match(x.Name, @"\d{8}").Value);

            if (onlineModels != null)
            {
                var newestLangpairModel = onlineModels.LastOrDefault(x => x.Name.Contains($"{sourceLang}-{targetLang}"));

                if (newestLangpairModel != null)
                {
                    var newestModelDir = Path.Combine(
                    FiskmoAppdataDir.FullName, Regex.Replace(newestLangpairModel.Name, @"\.zip$", ""));
                    if (!Directory.Exists(newestModelDir))
                    {
                        newerModel = newestLangpairModel.Name;
                    }
                }
            }

            return newerModel;
        }
        
    }
}
