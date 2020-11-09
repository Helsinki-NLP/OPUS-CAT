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
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Threading;
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
        { get => filteredOnlineModels; set { filteredOnlineModels = value; NotifyPropertyChanged(); } }
        
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

        internal List<string> GetLanguagePairModelTags(string languagePair)
        {
            var sourceTargetSplit = languagePair.Split('-');
            var sourceLang = sourceTargetSplit[0];
            var targetLang = sourceTargetSplit[1];
            var relevantModels = from m in this.LocalModels
                                 where m.SourceLanguages.Contains(sourceLang)
                                 where m.TargetLanguages.Contains(targetLang)
                                 where m.ModelConfig != null
                                 select m.ModelConfig.ModelTags;

            return relevantModels.SelectMany(x => x).ToList();
        }

        private DirectoryInfo opusModelDir;
        private FileSystemWatcher watcher;
        private bool batchTranslationOngoing;
        private bool customizationOngoing;

        public DirectoryInfo OpusModelDir { get => opusModelDir; }
        public bool CustomizationOngoing { get => customizationOngoing; set { customizationOngoing = value; NotifyPropertyChanged(); } }
        public bool BatchTranslationOngoing { get => batchTranslationOngoing; set { batchTranslationOngoing = value; NotifyPropertyChanged(); } }

        internal string CheckModelStatus(string sourceCode, string targetCode, string modelTag)
        {
            StringBuilder statusMessage = new StringBuilder();
            var primaryModel = this.GetPrimaryModel(sourceCode, targetCode);
            if (primaryModel == null)
            {
                statusMessage.Append($"No model available for {sourceCode}-{targetCode}. Install a model in the Fiskmö MT engine application.");
            }
            else if (modelTag != null && modelTag != "")
            {
                var taggedModel = this.GetModelByTag(modelTag, sourceCode, targetCode, true);
                if (taggedModel == null)
                {
                    statusMessage.Append($"No model with tag {modelTag} available for {sourceCode}-{targetCode}. ");
                    statusMessage.Append($"Fine-tuning may have been aborted, or the model has been deleted. ");
                    statusMessage.Append($"Primary model {primaryModel.Name} for {sourceCode}-{targetCode} will be used.");
                }
                else if (taggedModel.Status == MTModelStatus.Customizing)
                {
                    statusMessage.Append($"Model with tag {modelTag} for {sourceCode}-{targetCode} is still being fine-tuned. ");
                    statusMessage.Append($"Wait for fine-tuning to complete. If Fiskmö MT is used before the fine-tuning is complete, primary model {primaryModel.Name} for {sourceCode}-{targetCode} will be used.");
                }
                else
                {
                    statusMessage.Append($"Model with tag {modelTag} for {sourceCode}-{targetCode} is available. ");
                }
            }
            else
            {
                statusMessage.Append($"Primary model {primaryModel.Name} for {sourceCode}-{targetCode} will be used.");
            }

            return statusMessage.ToString();
        }

        internal void SortOnlineModels(string header, ListSortDirection direction)
        {
            var test = this.FilteredOnlineModels.First()[header];
            this.FilteredOnlineModels = new ObservableCollection<MTModel>(this.FilteredOnlineModels.OrderBy(x => x[header]));
            if (direction == ListSortDirection.Descending)
            {
                this.FilteredOnlineModels = new ObservableCollection<MTModel>(this.FilteredOnlineModels.Reverse());
            }
            NotifyPropertyChanged("FilteredOnlineModels");
        }


        internal void SortLocalModels(string header, ListSortDirection direction)
        {
            this.LocalModels = new ObservableCollection<MTModel>(this.LocalModels.OrderBy(x => x[header]));
            if (direction == ListSortDirection.Descending)
            {
                this.LocalModels = new ObservableCollection<MTModel>(this.LocalModels.Reverse());
            }
            NotifyPropertyChanged("LocalModels");
        }

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
                x => x.ModelPath.Replace("/", "\\") == selectedModel.ModelPath);
            if (onlineModel != null)
            {
                onlineModel.InstallStatus = "";
                onlineModel.InstallProgress = 0;
            }

            this.LocalModels.Remove(selectedModel);
            selectedModel.Shutdown();

            if (Directory.Exists(Path.Combine(this.opusModelDir.FullName, selectedModel.ModelPath)))
            {
                try
                {
                    FileSystem.DeleteDirectory(
                    Path.Combine(this.opusModelDir.FullName, selectedModel.ModelPath), UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Model directory can't be deleted, some other process is using it.", "Exception", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                
            }
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

        private MTModel SelectModel(string srcLangCode, string trgLangCode, string modelTag, bool includeIncomplete = false)
        {

            MTModel mtModel;

            if (modelTag == null || modelTag == "")
            {
                mtModel = this.GetPrimaryModel(srcLangCode, trgLangCode);
            }
            else
            {
                mtModel = this.GetModelByTag(modelTag, srcLangCode, trgLangCode, includeIncomplete);
                if (mtModel == null)
                {
                    mtModel = this.GetPrimaryModel(srcLangCode, trgLangCode);
                }
            }
            return mtModel;
        }

        internal void PreTranslateBatch(List<string> input, string srcLangCode, string trgLangCode, string modelTag)
        {
            this.BatchTranslationOngoing = true;
            var mtModel = this.SelectModel(srcLangCode, trgLangCode, modelTag, true);

            Log.Information($"Pretranslating a batch of translations with model tag {modelTag}, model {mtModel.Name} will be used.");
            if (mtModel.Status == MTModelStatus.OK)
            {
                var batchProcess = mtModel.PreTranslateBatch(input);
                batchProcess.Exited += BatchProcess_Exited;
            }
            else
            {
                Log.Information($"Model {mtModel.Name} was not ready for translation, batch translation canceled.");
            }
        }

        private void BatchProcess_Exited(object sender, EventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() => this.BatchTranslationOngoing = false);
        }

        private MTModel GetModelByTag(string tag, string srcLangCode, string trgLangCode, bool includeIncomplete = false)
        {
            //There could be multiple models finetuned with the same tag, use the latest.
            if (includeIncomplete)
            {
                return this.LocalModels.FirstOrDefault(
                x =>
                    x.ModelConfig.ModelTags.Contains(tag) &&
                    x.SourceLanguages.Contains(srcLangCode) &&
                    x.TargetLanguages.Contains(trgLangCode));
            }
            else
            {
                return this.LocalModels.FirstOrDefault(
                x =>
                    x.ModelConfig.ModelTags.Contains(tag) &&
                    x.SourceLanguages.Contains(srcLangCode) &&
                    x.TargetLanguages.Contains(trgLangCode) &&
                    x.Status == MTModelStatus.OK);
            }


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

                //Remove multilanguage models from the list, they aren't supported yet
                this.onlineModels = this.onlineModels.Where(x => x.SourceLanguages.Count == 1 && x.TargetLanguages.Count == 1).ToList();
                this.FilterOnlineModels("", "", "");
            }
        }

        internal string TranslateWithModel(string input, string modelName)
        {
            return this.LocalModels.Single(x => x.Name == modelName).Translate(input);
        }

        internal void StartCustomization(
            ParallelFilePair inputPair,
            ParallelFilePair validationPair,
            List<string> uniqueNewSegments,
            string srcLangCode,
            string trgLangCode,
            string modelTag,
            bool includePlaceholderTags,
            bool includeTagPairs,
            DirectoryInfo customDir,
            MTModel baseModel)
        {
            var customTask = Task.Run(() => Customize(inputPair, validationPair, uniqueNewSegments, srcLangCode, trgLangCode, modelTag, includePlaceholderTags, includeTagPairs, customDir, baseModel));
            customTask.ContinueWith(taskExceptionHandler, TaskContinuationOptions.OnlyOnFaulted);
        }

        internal void StartCustomization(List<Tuple<string, string>> input,
            List<Tuple<string, string>> validation,
            List<string> uniqueNewSegments,
            string srcLangCode,
            string trgLangCode,
            string modelTag,
            bool includePlaceholderTags,
            bool includeTagPairs,
            MTModel baseModel=null)
        {
            if (baseModel == null)
            {
                baseModel = this.GetPrimaryModel(srcLangCode, trgLangCode);
            }

            var customDir = new DirectoryInfo($"{baseModel.InstallDir}_{modelTag}");
            
            /* this.watcher = new FileSystemWatcher(customDir.FullName, "valid.log");
             this.watcher.EnableRaisingEvents = true;
             this.watcher.Changed += finetuningProgressChanged;*/

            var customTask = Task.Run(() => Customize(input, validation, uniqueNewSegments, srcLangCode, trgLangCode, modelTag, includePlaceholderTags, includeTagPairs, customDir, baseModel));
            customTask.ContinueWith(taskExceptionHandler, TaskContinuationOptions.OnlyOnFaulted);
        }

        private void taskExceptionHandler(Task obj)
        {
            Log.Error($"Task failed due to the following exception: {obj.Exception}");
        }

        /// <summary>
        /// this monitors the progress of finetuning based on the 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /*private void finetuningProgressChanged(object sender, FileSystemEventArgs e)
        {
            ;
        }*/

        internal void Customize(
            List<Tuple<string, string>> input,
            List<Tuple<string, string>> validation,
            List<string> uniqueNewSegments,
            string srcLangCode,
            string trgLangCode,
            string modelTag,
            bool includePlaceholderTags,
            bool includeTagPairs,
            DirectoryInfo customDir,
            MTModel baseModel)
        {
            //Write the tuning set as two files
            var fileGuid = Guid.NewGuid();
            var srcFile = Path.Combine(Path.GetTempPath(), $"{fileGuid}.{srcLangCode}");
            var trgFile = Path.Combine(Path.GetTempPath(), $"{fileGuid}.{trgLangCode}");
            var validSrcFile = Path.Combine(Path.GetTempPath(), $"{fileGuid}.validation.{srcLangCode}");
            var validTrgFile = Path.Combine(Path.GetTempPath(), $"{fileGuid}.validation.{trgLangCode}");

            var inputPair = new ParallelFilePair(input, srcFile, trgFile);
            var validPair = new ParallelFilePair(validation, validSrcFile, validTrgFile);

            this.Customize(
                inputPair,
                validPair,
                uniqueNewSegments,
                srcLangCode,
                trgLangCode,
                modelTag,
                includePlaceholderTags,
                includeTagPairs,
                customDir,
                baseModel);
        }

        internal void Customize(
            ParallelFilePair inputPair,
            ParallelFilePair validationPair,
            List<string> uniqueNewSegments,
            string srcLangCode,
            string trgLangCode,
            string modelTag,
            bool includePlaceholderTags,
            bool includeTagPairs,
            DirectoryInfo customDir,
            MTModel baseModel)
        {

            Log.Information($"Customizing a new model with model tag {modelTag} from base model {baseModel.Name}.");
            //Add an entry for an incomplete model to the model list
            var modelPath = Regex.Match(customDir.FullName, @"[^\\]+\\[^\\]+$").Value;
            var incompleteModel = new MTModel(
                    $"{baseModel.Name}_{modelTag}",
                    modelPath,
                    srcLangCode,
                    trgLangCode,
                    MTModelStatus.Customizing,
                    modelTag,
                    customDir,
                    null,
                    includePlaceholderTags,
                    includeTagPairs);
            
            Application.Current.Dispatcher.Invoke(() =>
                this.LocalModels.Add(incompleteModel));

            //Note that this does not currently remove the temp files, should
            //add an event for that in the Marian process startup code
            //(but make sure that non-temp customization files are not removed).

            var customizer = new MarianCustomizer(
                baseModel,
                incompleteModel,
                inputPair,
                validationPair,
                modelTag,
                includePlaceholderTags,
                includeTagPairs,
                uniqueNewSegments
                );

            customizer.ProgressChanged += incompleteModel.CustomizationProgressHandler;
            customizer.ProcessExited += incompleteModel.ExitHandler;
            
            var trainProcess = customizer.Customize();

            //Add process to model and save its config (the directory exists at this point, 
            //so config can be saved).
            incompleteModel.FinetuneProcess = trainProcess;
            incompleteModel.SaveModelConfig();

            this.CustomizationOngoing = true;    
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

        internal string Translate(string input, string srcLangCode, string trgLangCode, string modelTag)
        {

            var mtModel = this.SelectModel(srcLangCode, trgLangCode, modelTag);

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
            if (languagePairModels.Any())
            {
                var prioritizedModels = languagePairModels.Where(x => x.Prioritized);
                if (prioritizedModels.Any())
                {
                    primaryModel = prioritizedModels.First();
                }
                else
                {
                    //Pick a model that is not fine-tuned
                    primaryModel = languagePairModels.FirstOrDefault(x => !x.ModelConfig.Finetuned);
                    if (primaryModel == null)
                    {
                        //As a fallback pick any model
                        primaryModel = languagePairModels.First();
                    }
                }

                return primaryModel;
            }
            else
            {
                return null;
            }
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
            var downloadPath = Path.Combine(this.OpusModelDir.FullName, newerModel + ".zip");

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
        internal void ExtractModel(string modelPath, bool deleteZip = false)
        {
            string modelFolder = Path.Combine(this.OpusModelDir.FullName, modelPath);
            string zipPath = modelFolder + ".zip";

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
                Directory.Delete(tempExtractionPath, true);
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
            using (var readme = new StreamReader(Path.Combine(tempExtractionPath, "README.md"), Encoding.UTF8))
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
                Directory.Delete(tempExtractionPath, true);
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
