using Microsoft.VisualBasic.FileIO;
using Serilog;
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
using System.Windows;
using System.Xml.Linq;
using static System.Environment;

namespace FiskmoMTEngine
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


        internal void UninstallModel(MTModel selectedModel)
        {
            var onlineModel = this.onlineModels.SingleOrDefault(
                x => x.ModelPath.Replace("/","\\") == selectedModel.ModelPath);
            if (onlineModel != null)
            {
                onlineModel.InstallStatus = "";
                onlineModel.InstallProgress = 0;
            }
            
            this.LocalModels.Remove(selectedModel);
            selectedModel.Shutdown();
            FileSystem.DeleteDirectory(
                Path.Combine(this.opusModelDir.FullName,selectedModel.ModelPath),UIOption.OnlyErrorDialogs,RecycleOption.SendToRecycleBin);
        }

        public ModelManager()
        {
             
            this.GetOnlineModels();
            this.opusModelDir = new DirectoryInfo(Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                FiskmoMTEngineSettings.Default.LocalFiskmoDir,
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

            Log.Information($"Fetching a list of online models from {FiskmoMTEngineSettings.Default.ModelStorageUrl}");
            using (var client = new WebClient())
            {
                client.DownloadStringCompleted += modelListDownloadComplete;
                client.DownloadStringAsync(new Uri(FiskmoMTEngineSettings.Default.ModelStorageUrl));
            }
        }

        internal void PreTranslateBatch(List<string> input, string srcLangCode, string trgLangCode, string modelTag)
        {
            MTModel mtModel;

            if (modelTag == null)
            {
                mtModel = this.GetPrimaryModel(srcLangCode, trgLangCode);
            }
            else
            {
                mtModel = this.GetModelByTag(modelTag);
                if (mtModel == null)
                {
                    mtModel = this.GetPrimaryModel(srcLangCode, trgLangCode);
                }
            }

            mtModel.PreTranslateBatch(input);
        }

        private MTModel GetModelByTag(string tag)
        {
            //There could be multiple models finetuned with the same tag, use the latest.
            return this.LocalModels.FirstOrDefault(x => x.ModelTags.Contains(tag));
        }

        private void modelListDownloadComplete(object sender, DownloadStringCompletedEventArgs e)
        {
            XDocument bucket;
            try
            {
                bucket = XDocument.Parse(e.Result);
            }
            catch (Exception ex) when (ex.InnerException is System.Net.WebException)
            {
                Log.Information($"Could not connect to online model storage, check that Internet connection exists. Exception: {ex.InnerException.ToString()}");

                return;
            }
            
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
                    var uriBuilder = new UriBuilder(FiskmoMTEngineSettings.Default.ModelStorageUrl);
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
                    var localModelPaths = this.LocalModels.Select(x => x.ModelPath.Replace("\\", "/"));
                    if (localModelPaths.Contains(onlineModel.ModelPath))
                    {
                        onlineModel.InstallStatus = "Installed";
                        onlineModel.InstallProgress = 100;
                    }
                }

                this.FilterOnlineModels("", "", "");
            }
        }

        internal string TranslateWithModel(string input, string modelName)
        {
            return this.LocalModels.Single(x => x.Name == modelName).Translate(input);
        }

        private void SplitToFiles(List<Tuple<string, string>> biText,string srcPath, string trgPath)
        {
            using (var srcStream = new StreamWriter(srcPath, true, Encoding.UTF8))
            using (var trgStream = new StreamWriter(trgPath, true, Encoding.UTF8))
            {
                foreach (var pair in biText)
                {
                    srcStream.WriteLine(pair.Item1);
                    trgStream.WriteLine(pair.Item2);
                }
            }
        }

        internal void Customize(List<Tuple<string, string>> input, List<Tuple<string, string>> validation,string srcLangCode, string trgLangCode)
        {
            var primaryModel = this.GetPrimaryModel(srcLangCode, trgLangCode);

            //Write the tuning set as two files
            var fileGuid = Guid.NewGuid();
            var srcFile = Path.Combine(Path.GetTempPath(), $"{fileGuid}.{srcLangCode}");
            var trgFile = Path.Combine(Path.GetTempPath(), $"{fileGuid}.{trgLangCode}");
            var validSrcFile = Path.Combine(Path.GetTempPath(), $"{fileGuid}.validation.{srcLangCode}");
            var validTrgFile = Path.Combine(Path.GetTempPath(), $"{fileGuid}.validation.{trgLangCode}");

            this.SplitToFiles(input, srcFile, trgFile);
            this.SplitToFiles(validation, validSrcFile, validTrgFile);

            //Note that this does not currently remove the temp files, should
            //add an event for that in the Marian process startup code
            //(but make sure that non-temp customization files are not removed).

            var customizer = new MarianCustomizer(
                primaryModel,
                new FileInfo(srcFile),
                new FileInfo(trgFile),
                new FileInfo(validSrcFile),
                new FileInfo(validTrgFile),
                "customized"
                );

            customizer.Customize();
            
        }

        internal void GetLocalModels()
        {
            if (this.LocalModels == null)
            {
                this.LocalModels = new ObservableCollection<MTModel>();
            }

            var modelPaths = this.OpusModelDir.GetFiles("*.npz", System.IO.SearchOption.AllDirectories).Select(x => x.DirectoryName).Distinct().ToList();
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
            //Strip region code if there is one
            name = Regex.Replace(name, "-[A-Z]{2}", "");

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

            MTModel mtModel;
            mtModel = this.GetPrimaryModel(srcLangCode, trgLangCode);
            
            return mtModel.Translate(input);
        }

        private MTModel GetPrimaryModel(string srcLangCode, string trgLangCode)
        {
            //The language codes can be 3 or 2 letter formats, and the code may contain a
            //region specifier (e.g. swe-FI). Normalize everything as 2 letter codes for now.

            var threeLetterRegex = new Regex("^[a-z]{3}(-[A-Z]{2})?$");
            if (threeLetterRegex.IsMatch(srcLangCode))
            {
                srcLangCode = this.ConvertIsoCode(srcLangCode);
            }

            if (threeLetterRegex.IsMatch(trgLangCode))
            {
                trgLangCode = this.ConvertIsoCode(trgLangCode);
            }

            var languagePairModels = this.LocalModels.Where(x => x.SourceLanguages.Contains(srcLangCode) && x.TargetLanguages.Contains(trgLangCode));
            MTModel primaryModel;
            var prioritizedModels = languagePairModels.Where(x => x.Prioritized);
            if (prioritizedModels.Any())
            {
                primaryModel = prioritizedModels.First();
            }
            else
            {
                primaryModel = languagePairModels.First();
            }
            
            return primaryModel;
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
                var modelUrl = $"{FiskmoMTEngineSettings.Default.ModelStorageUrl}{newerModel}.zip";
                Directory.CreateDirectory(Path.GetDirectoryName(downloadPath));
                client.DownloadFileAsync(new Uri(modelUrl), downloadPath);
            }
        }

        //This is used with automatic downloads from the object storage, where the
        //language pair is contained in the object storage path
        internal void ExtractModel(string modelPath,bool deleteZip=false)
        {
            string modelFolder = Path.Combine(this.OpusModelDir.FullName, modelPath);
            string zipPath = modelFolder+".zip";
            
            ZipFile.ExtractToDirectory(zipPath, modelFolder);
            if (deleteZip)
            {
                File.Delete(zipPath);
            }
        }

        //This is used for extraction from a zip, where the language pair needs to
        //be extracted from the README.md file (some nicer metadata file might be useful)
        internal void ExtractModel(FileInfo zipFile)
        {
            var tempExtractionPath = Path.Combine(Path.GetTempPath(), zipFile.Name);
            if (Directory.Exists(tempExtractionPath))
            {
                Directory.Delete(tempExtractionPath,true);
            }

            try
            {
                ZipFile.ExtractToDirectory(zipFile.FullName, tempExtractionPath);
            }
            catch (System.IO.InvalidDataException ex)
            {
                MessageBox.Show($"Installation from a model zip failed. The zip archive is invalid.", "Exception", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string languagePairs;
            using (var readme = new StreamReader(Path.Combine(tempExtractionPath, "README.md"),Encoding.UTF8))
            {
                var readmeText = readme.ReadToEnd();
                var regex = @"\* download.+/([a-z]{2,3}-[a-z]{2,3})/.+\.zip\)";
                var match = Regex.Match(readmeText, regex);
                languagePairs = match.Groups[1].Value;
            }

            var destinationDirectory = Path.Combine(this.opusModelDir.FullName, languagePairs);
            if (!Directory.Exists(destinationDirectory))
            {
                Directory.CreateDirectory(destinationDirectory);
            }

            var modelDir = Path.Combine(destinationDirectory, zipFile.Name.Replace(".zip", ""));
            if (Directory.Exists(modelDir))
            {
                MessageBox.Show($"A model with this name and language direction has already been installed.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                Directory.Delete(tempExtractionPath,true);
            }
            else
            {
                Directory.Move(tempExtractionPath, modelDir);
            }
            

            this.GetLocalModels();
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
