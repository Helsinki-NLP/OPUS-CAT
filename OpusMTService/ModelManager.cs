using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Linq;
using static System.Environment;

namespace OpusMTService
{
    /// <summary>
    /// This class contains methods for checking and downloading latest models from
    /// the fiskmo model repository. 
    /// </summary>
    public class ModelManager : INotifyPropertyChanged
    {

        private List<MTModel> onlineModels;

        private ObservableCollection<MTModel> filteredOnlineModels = new ObservableCollection<MTModel>();

        public ObservableCollection<MTModel> FilteredOnlineModels
        { get => filteredOnlineModels; set { filteredOnlineModels= value; NotifyPropertyChanged(); } }

        private ObservableCollection<MTModel> localModels;

        public ObservableCollection<MTModel> LocalModels
        { get => localModels; set { localModels = value; NotifyPropertyChanged(); } }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        

        private DirectoryInfo opusModelDir;

        public DirectoryInfo OpusModelDir { get => opusModelDir; }

        internal void FilterOnlineModels(string sourceFilter, string targetFilter, string nameFilter)
        {
            var filteredModels = from model in this.onlineModels
                                 where
                                    model.SourceLanguageString.Contains(sourceFilter) &&
                                    model.TargetLanguageString.Contains(targetFilter) &&
                                    model.Name.Contains(nameFilter)
                                 select model;

            this.FilteredOnlineModels.Clear();
            foreach (var model in filteredModels)
            {
                this.FilteredOnlineModels.Add(model);
            }
        }

        public ModelManager()
        {
             
            this.GetOnlineModels();
            this.opusModelDir = new DirectoryInfo(Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                OpusMTServiceSettings.Default.LocalFiskmoDir,
                "models"));
            if (!this.OpusModelDir.Exists)
            {
                this.OpusModelDir.Create();
            }

            this.GetLocalModels();
        }

        private void GetOnlineModels()
        {
            this.onlineModels = new List<MTModel>();

            using (var client = new WebClient())
            {
                client.DownloadStringCompleted += modelListDownloadComplete;
                client.DownloadStringAsync(new Uri(OpusMTServiceSettings.Default.ModelStorageUrl));
            }
        }

        private void modelListDownloadComplete(object sender, DownloadStringCompletedEventArgs e)
        {
            XDocument bucket = XDocument.Parse(e.Result);
            var ns = bucket.Root.Name.Namespace;
            var files = bucket.Descendants(ns + "Key").Select(x => x.Value);
            var models = files.Where(x => Regex.IsMatch(x, @"[^/-]+-[^/-]+/[^.]+\.zip"));
            this.onlineModels.AddRange(models.Select(x => new MTModel(x.Replace("models /", "").Replace(".zip", ""))));

            var nextMarker = bucket.Descendants(ns + "NextMarker").SingleOrDefault();
            if (nextMarker != null)
            {
                using (var client = new WebClient())
                {
                    client.DownloadStringCompleted += modelListDownloadComplete;
                    var uriBuilder = new UriBuilder(OpusMTServiceSettings.Default.ModelStorageUrl);
                    var query = HttpUtility.ParseQueryString(uriBuilder.Query);
                    query["marker"] = nextMarker.Value;
                    uriBuilder.Query = query.ToString();
                    client.DownloadStringAsync(uriBuilder.Uri);
                }
            }

            else
            {
                foreach (var onlineModel in this.onlineModels)
                {
                    var localModelPaths = this.LocalModels.Select(x => x.Path.Replace("\\", "/"));
                    if (localModelPaths.Contains(onlineModel.Path))
                    {
                        onlineModel.InstallStatus = "Installed";
                        onlineModel.InstallProgress = 100;
                    }
                }

                this.FilterOnlineModels("", "", "");
            }
        }
        
        internal void GetLocalModels()
        {
            if (this.LocalModels == null)
            {
                this.LocalModels = new ObservableCollection<MTModel>();
            }

            var modelPaths = this.OpusModelDir.GetFiles("*.npz", SearchOption.AllDirectories).Select(x => x.DirectoryName).Distinct().ToList();
            this.LocalModels.Clear();

            foreach (var modelPath in modelPaths)
            {
                this.LocalModels.Add(new MTModel(Regex.Match(modelPath, @"[^\\]+\\[^\\]+$").Value, modelPath));
            }
        }

        internal string[] GetAllModelDirs(string sourceLang, string targetLang)
        {
            if (Directory.Exists(
                Path.Combine(this.opusModelDir.FullName, "models", $"{sourceLang}-{targetLang}")))
            {
                var languagePairModels = Directory.GetDirectories(Path.Combine(
                    this.opusModelDir.FullName, "models", $"{sourceLang}-{targetLang}"));
                return languagePairModels;
            }
            else
            {
                return null;
            }
        }

        public string ConvertIsoCode(string name)
        {
            if (name.Length != 3)
            {
                throw new ArgumentException("name must be three letters.");
            }

            name = name.ToLower();

            CultureInfo[] cultures = CultureInfo.GetCultures(CultureTypes.SpecificCultures);
            foreach (CultureInfo culture in cultures)
            {
                if (culture.ThreeLetterISOLanguageName.ToLower() == name)
                {
                    return culture.TwoLetterISOLanguageName.ToLower();
                }
            }

            return null;
        }

        internal string Translate(string input, string srcLangCode, string trgLangCode)
        {
            //Use the first suitable model
            if (srcLangCode.Length == 3)
            {
                srcLangCode = this.ConvertIsoCode(srcLangCode);
            }

            if (trgLangCode.Length == 3)
            {
                trgLangCode = this.ConvertIsoCode(trgLangCode);
            }
            var installedModel = this.LocalModels.Where(x => x.SourceLanguages.Contains(srcLangCode) && x.TargetLanguages.Contains(trgLangCode)).First();
            return installedModel.Translate(input);
        }

        internal IEnumerable<string> GetAllLanguagePairs()
        {
            var languagePairs = new List<string>();
            foreach (var model in this.LocalModels)
            {
                var modelLanguagePairs = from sourceLang in model.SourceLanguages
                                    from targetLang in model.TargetLanguages
                                    select $"{sourceLang}-{targetLang}";
                languagePairs.AddRange(modelLanguagePairs);
            }

            return languagePairs.Distinct();
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
            var downloadPath = Path.Combine(this.OpusModelDir.FullName, newerModel+".zip");

            using (var client = new WebClient())
            {
                client.DownloadProgressChanged += wc_DownloadProgressChanged;
                client.DownloadFileCompleted += wc_DownloadComplete;
                var modelUrl = $"{OpusMTServiceSettings.Default.ModelStorageUrl}{newerModel}.zip";
                Directory.CreateDirectory(Path.GetDirectoryName(downloadPath));
                client.DownloadFileAsync(new Uri(modelUrl), downloadPath);
            }
        }

        internal void ExtractModel(string modelPath,bool deleteZip=false)
        {
            string modelFolder = Path.Combine(this.OpusModelDir.FullName, modelPath);
            string zipPath = modelFolder+".zip";

            this.ExtractModel(new FileInfo(zipPath), deleteZip);
        }

        //For extracting zip files
        internal void ExtractModel(FileInfo zipPath, bool deleteZip = false)
        {
            Match modelPathMatch = Regex.Match(zipPath.FullName, @".*\\([^\\]+\\[^\\]+)\.zip$");
            string modelPath = modelPathMatch.Groups[1].Value;
            string modelFolder = Path.Combine(this.OpusModelDir.FullName, modelPath);
            ZipFile.ExtractToDirectory(zipPath.FullName, modelFolder);
            if (deleteZip)
            {
                zipPath.Delete();
            }
        }


        public Boolean IsLanguagePairSupported(string sourceLang, string targetLang)
        {
            //This method is called many times (possibly for each segment), so use the cached model list instead of
            //new model fetch call to check whether pair is supported
            return this.LocalModels.Any(x => x.Name.Contains($"{sourceLang}-{targetLang}"));
        }

        //Check for existence of current Fiskmo models in ProgramData
        public string CheckForNewerModel(string sourceLang, string targetLang)
        {
            string newerModel = null;
            //var timestamps = this.GetLatestModelInfo().Select(x => Regex.Match(x, @"\d{8}").Value);
            //order models by timestamp
            var onlineModels = this.onlineModels.OrderBy(x => Regex.Match(x.Name, @"\d{8}").Value);

            if (onlineModels != null)
            {
                var newestLangpairModel = onlineModels.LastOrDefault(x => x.Name.Contains($"{sourceLang}-{targetLang}"));

                if (newestLangpairModel != null)
                {
                    var newestModelDir = Path.Combine(
                    OpusModelDir.FullName, Regex.Replace(newestLangpairModel.Name, @"\.zip$", ""));
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
