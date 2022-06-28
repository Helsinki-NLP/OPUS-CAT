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
using System.ServiceModel;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using System.Xml.Linq;
using YamlDotNet.Serialization;
using static System.Environment;

namespace OpusCatMTEngine
{
    /// <summary>
    /// This class contains methods for checking and downloading latest models from
    /// the opus cat model repository and managing the downloaded models. 
    /// </summary>
    public class ModelManager : INotifyPropertyChanged, IMtProvider
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

        public List<string> GetLanguagePairModelTags(string sourceCode, string targetCode)
        {
            var sourceLang = new IsoLanguage(sourceCode);
            var targetLang = new IsoLanguage(targetCode);
            var relevantModels = from m in this.LocalModels
                                 where m.SourceLanguages.Any(x => x.IsCompatibleLanguage(sourceLang))
                                 where m.TargetLanguages.Any(x => x.IsCompatibleLanguage(targetLang))
                                 where m.ModelConfig != null
                                 select m.ModelConfig.ModelTags;

            return relevantModels.SelectMany(x => x).ToList();
        }

        private DirectoryInfo opusModelDir;
        private bool batchTranslationOngoing;

        public bool OverrideModelSet { get => this.overrideModel != null; }
        public MTModel OverrideModel
        {
            get => overrideModel;
            set
            {
                overrideModel = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged("OverrideModelSet");
            }
        }

        private MTModel overrideModel;
        private bool _showBilingualModels;
        private bool _showMultilingualModels;
        private bool _showOpusModels;
        private bool _showTatoebaModels;
        private string _sourceFilter;
        private string _targetFilter;
        private string _nameFilter;
        private HashSet<Uri> yamlDownloads;
        private bool _showNewestOnly;
        private bool _showFaulted;

        public DirectoryInfo OpusModelDir { get => opusModelDir; }
        public bool FinetuningOngoing
        {
            get => this.LocalModels.Any(x => x.Status == MTModelStatus.Finetuning);
        }
        public bool BatchTranslationOngoing { get => batchTranslationOngoing; set { batchTranslationOngoing = value; NotifyPropertyChanged(); } }

        public bool ShowBilingualModels
        {
            get => _showBilingualModels;
            set
            {
                _showBilingualModels = value;
                NotifyPropertyChanged();
                this.FilterOnlineModels();
            }
        }

        public bool ShowTransformerBigModels
        {
            get => _showTransformerBigModels;
            set
            {
                _showTransformerBigModels = value;
                NotifyPropertyChanged();
                this.FilterOnlineModels();
            }
        }

        public bool ShowMultilingualModels
        {
            get => _showMultilingualModels;
            set
            {
                _showMultilingualModels = value;
                NotifyPropertyChanged();
                this.FilterOnlineModels();
            }
        }

        public bool ShowOpusModels
        {
            get => _showOpusModels;
            set
            {
                _showOpusModels = value;
                NotifyPropertyChanged();
                this.FilterOnlineModels();
            }
        }

        public bool ShowNewestOnly
        {
            get => _showNewestOnly;
            set
            {
                _showNewestOnly = value;
                NotifyPropertyChanged();
                this.FilterOnlineModels();
            }
        }

        public bool ShowFaulted
        {
            get => _showFaulted;
            set
            {
                _showFaulted = value;
                NotifyPropertyChanged();
                this.FilterOnlineModels();
            }
        }

        public bool ShowTatoebaModels
        {
            get => _showTatoebaModels;
            set
            {
                _showTatoebaModels = value;
                NotifyPropertyChanged();
                this.FilterOnlineModels();
            }
        }

        public string SourceFilter
        {
            get => _sourceFilter;
            set
            {
                _sourceFilter = value;
                NotifyPropertyChanged();
                this.FilterOnlineModels();
            }
        }

        public string TargetFilter
        {
            get => _targetFilter;
            set
            {
                _targetFilter = value;
                NotifyPropertyChanged();
                this.FilterOnlineModels();
            }
        }

        public string NameFilter
        {
            get => _nameFilter;
            set
            {
                _nameFilter = value;
                NotifyPropertyChanged();
                this.FilterOnlineModels();
            }
        }

        public bool OnlineModelListFetched
        {
            get => onlineModelListFetched;
            set
            {
                onlineModelListFetched = value;
                NotifyPropertyChanged();
            }
        }

        public IsoLanguage OverrideModelTargetLanguage
        {
            get => _overrideModelTargetLanguage;
            set
            {
                _overrideModelTargetLanguage = value;
                NotifyPropertyChanged();
            }
        }

        internal ObservableCollection<AutoEditRuleCollection> AutoPreEditRuleCollections { get; private set; }
        internal ObservableCollection<AutoEditRuleCollection> AutoPostEditRuleCollections { get; private set; }

        private bool onlineModelListFetched;
        private IsoLanguage _overrideModelTargetLanguage;
        private bool _showTransformerBigModels;

        public string CheckModelStatus(IsoLanguage sourceLang, IsoLanguage targetLang, string modelTag)
        {
            var sourceCode = sourceLang.ShortestIsoCode;
            var targetCode = targetLang.ShortestIsoCode;
            StringBuilder statusMessage = new StringBuilder();
            var primaryModel = this.GetPrimaryModel(sourceLang, targetLang);
            if (primaryModel == null)
            {
                statusMessage.Append($"No model available for {sourceCode}-{targetCode}. Install a model in the OPUS-CAT MT Engine.");
            }
            else if (modelTag != null && modelTag != "")
            {
                var taggedModel = this.GetModelByTag(modelTag, sourceLang, targetLang, true);
                if (taggedModel == null)
                {
                    statusMessage.Append($"No model with tag {modelTag} available for {sourceCode}-{targetCode}. ");
                    statusMessage.Append($"Fine-tuning may have been aborted, or the model has been deleted. ");
                    statusMessage.Append($"Primary model {primaryModel.Name} for {sourceCode}-{targetCode} will be used.");
                }
                else if (taggedModel.Status == MTModelStatus.Finetuning)
                {
                    statusMessage.Append($"Model with tag {modelTag} for {sourceCode}-{targetCode} is still being fine-tuned. ");
                    statusMessage.Append($"Wait for fine-tuning to complete. If OPUS-CAT MT is used before the fine-tuning is complete, primary model {primaryModel.Name} for {sourceCode}-{targetCode} will be used.");
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
            this.FilteredOnlineModels = new ObservableCollection<MTModel>(this.FilteredOnlineModels.OrderBy(x => x[header]));
            if (direction == ListSortDirection.Descending)
            {
                this.FilteredOnlineModels = new ObservableCollection<MTModel>(this.FilteredOnlineModels.Reverse());
            }
            NotifyPropertyChanged("FilteredOnlineModels");
        }

        internal void MoveOverrideToTop()
        {
            var overrideModelIndex = this.localModels.IndexOf(this.OverrideModel);
            this.localModels.Move(overrideModelIndex, 0);
            NotifyPropertyChanged("LocalModels");
        }

        internal void SortLocalModels(string header, ListSortDirection direction)
        {
            this.LocalModels = new ObservableCollection<MTModel>(this.LocalModels.OrderBy(x => x[header]));
            if (direction == ListSortDirection.Descending)
            {
                this.LocalModels = new ObservableCollection<MTModel>(this.LocalModels.Reverse());
            }

            //If there's an override model, always show it on top
            if (this.OverrideModel != null)
            {
                this.MoveOverrideToTop();
            }

            NotifyPropertyChanged("LocalModels");
        }

        internal void FilterOnlineModels()
        {
            if (this.onlineModels == null)
            {
                return;
            }

            var filteredModels = from model in this.onlineModels
                                 where
                                    model.SourceLanguageString.ToLower().Contains(this.SourceFilter.ToLower()) &&
                                    model.TargetLanguageString.ToLower().Contains(this.TargetFilter.ToLower()) &&
                                    model.Name.ToLower().Contains(this.NameFilter.ToLower()) &&
                                    ((this.ShowOpusModels && model.ModelOrigin.Contains("OPUS-MT")) ||
                                    (this.ShowTatoebaModels && model.ModelOrigin.Contains("Tatoeba-MT")))
                                 select model;


            if (!this.ShowTransformerBigModels)
            {
                filteredModels = filteredModels.Where(x => !x.ModelType.Contains("transformer-big") && !x.ModelType.Contains("transformer-tiny"));
            }

            if (!this.ShowBilingualModels)
            {
                filteredModels = filteredModels.Where(x => (x.SourceLanguages.Count > 1 || x.TargetLanguages.Count > 1) || x.Faulted);
            }

            if (!this.ShowMultilingualModels)
            {
                filteredModels = filteredModels.Where(x => (x.SourceLanguages.Count == 1 && x.TargetLanguages.Count == 1) || x.Faulted);
            }

            if (!this.ShowFaulted)
            {
                filteredModels = filteredModels.Where(x => !x.Faulted);
            }

            if (this.ShowNewestOnly)
            {
                var onlyNew = new Dictionary<string, MTModel>();
                foreach (var model in filteredModels)
                {
                    var modelKey = model.ModelBaseName + model.SourceLanguageString + model.TargetLanguageString;
                    if (onlyNew.ContainsKey(modelKey))
                    {
                        if (onlyNew[modelKey].ModelDate < model.ModelDate)
                        {
                            onlyNew[modelKey] = model;
                        }
                    }
                    else
                    {
                        onlyNew[modelKey] = model;
                    }
                }

                filteredModels = onlyNew.Values;
            }

            this.FilteredOnlineModels.Clear();
            foreach (var model in filteredModels)
            {
                this.FilteredOnlineModels.Add(model);
            }
        }


        internal void UninstallModel(MTModel selectedModel)
        {
            //change the model status in online models, if they have been loaded
            if (this.onlineModels != null)
            {
                var onlineModel = this.onlineModels.SingleOrDefault(
                    x => x.ModelPath.Replace("/", "\\") == selectedModel.ModelPath);
                if (onlineModel != null)
                {
                    onlineModel.InstallStatus = "";
                    onlineModel.InstallProgress = 0;
                }
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

        private void InitializeAutoEditRuleCollections()
        {
            this.AutoPostEditRuleCollections = new ObservableCollection<AutoEditRuleCollection>();
            this.AutoPreEditRuleCollections = new ObservableCollection<AutoEditRuleCollection>();

            var editRuleDir = new DirectoryInfo(
                HelperFunctions.GetOpusCatDataPath(OpusCatMTEngineSettings.Default.EditRuleDir));

            if (!editRuleDir.Exists)
            {
                editRuleDir.Create();
            }
            var deserializer = new Deserializer();
            foreach (var file in editRuleDir.EnumerateFiles())
            {
                using (var reader = file.OpenText())
                {
                    try
                    {
                        var loadedRuleCollection = deserializer.Deserialize<AutoEditRuleCollection>(reader);
                        switch (loadedRuleCollection.CollectionType)
                        {
                            case "preedit":
                                this.AutoPreEditRuleCollections.Add(loadedRuleCollection);
                                break;
                            case "postedit":
                                this.AutoPostEditRuleCollections.Add(loadedRuleCollection);
                                break;
                            default:
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"Auto edit rule deserialization failed, file {file.Name}. Exception: {ex.Message}");
                    }
                }
            }
            
        }

        public ModelManager()
        {
            this.SourceFilter = "";
            this.TargetFilter = "";
            this.NameFilter = "";
            this.ShowBilingualModels = true;
            this.ShowMultilingualModels = false;
            this.ShowOpusModels = true;
            this.ShowTatoebaModels = true;
            this.ShowNewestOnly = true;

            this.InitializeAutoEditRuleCollections();

            var modelDirPath = HelperFunctions.GetOpusCatDataPath(OpusCatMTEngineSettings.Default.ModelDir);
            this.opusModelDir = new DirectoryInfo(modelDirPath);
            if (!this.OpusModelDir.Exists)
            {
                this.OpusModelDir.Create();
            }
            
            this.GetLocalModels();
        }
        
        internal void GetOnlineModels()
        {
            this.onlineModels = new List<MTModel>();
            this.yamlDownloads = new HashSet<Uri>();
            this.OnlineModelListFetched = false;
            var modelStorages =
                new List<string> {
                    OpusCatMTEngineSettings.Default.OpusModelStorageUrl//,
                    //OpusCatMTEngineSettings.Default.TatoebaModelStorageUrl
                };

            foreach (var modelStorage in modelStorages)
            {
                Log.Information($"Fetching a list of online models from {modelStorage}");

                //There might be a .NET library suitable for accessing Allas (did not find suitable one with quick search), 
                //but this works now, so stick with it.
                using (var client = new WebClient())
                {
                    var storageUri = new Uri(modelStorage);
                    client.DownloadStringCompleted += (x, y) => modelListDownloadComplete(storageUri, new List<string>(), x, y);
                    client.DownloadStringAsync(storageUri);
                }
            }

            //Tatoeba model info can be fetched as a single file from Allas
            using (var client = new WebClient())
            {
                var storageUrl = OpusCatMTEngineSettings.Default.TatoebaModelStorageUrl;
                var modelListUri = new Uri($"{storageUrl}released-model-languages.txt");
                client.DownloadStringCompleted += TatoebaModelListDownloaded;
                client.DownloadStringAsync(modelListUri);
            }

        }

        private void TatoebaModelListDownloaded(object sender, DownloadStringCompletedEventArgs e)
        {
            var modelList = e.Result;
            

            //Model list uses \n as line break, it originates form linux
            //Use distinct to remove duplicate entries
            foreach (var line in modelList.Split('\n').Distinct())
            {
                var split = line.Split('\t');
                if (split.Length >= 4)
                {
                    var modelPath = split[0];
                    var modelUri = new Uri($"{OpusCatMTEngineSettings.Default.TatoebaModelStorageUrl}{modelPath}");
                    var modelType = split[1];
                    IEnumerable<string> sourceLangs = split[2].Split(',');
                    IEnumerable<string> targetLangs = split[3].Split(',');

                    //Some entries might have empty source and target languages
                    if (!sourceLangs.Any() || !targetLangs.Any())
                    {
                        continue;
                    }
                    var model = new MTModel(modelPath.Replace(".zip", ""), modelUri);
                    model.ModelType = modelType;
                    model.SourceLanguages = sourceLangs.Select(x => new IsoLanguage(x)).ToList();
                    model.TargetLanguages = targetLangs.Select(x => new IsoLanguage(x)).ToList();
                    this.onlineModels.Add(model);
                }
            }
        }

        private MTModel SelectModel(IsoLanguage srcLang, IsoLanguage trgLang, string modelTag, bool includeIncomplete = false)
        {

            MTModel mtModel;

            //It's possible to choose an override model, which is used to serve all requests.
            //This is intended for testing and for those cases where CAT tools / 3rd party software
            //uses non-standard language codes (e.g. memoq and cgy for Montenegrin).
            if (this.OverrideModel != null)
            {
                mtModel = this.OverrideModel;
            }
            else if (modelTag == null || modelTag == "")
            {
                mtModel = this.GetPrimaryModel(srcLang, trgLang);
            }
            else
            {
                mtModel = this.GetModelByTag(modelTag, srcLang, trgLang, includeIncomplete);
                if (mtModel == null)
                {
                    mtModel = this.GetPrimaryModel(srcLang, trgLang);
                }
            }
            return mtModel;
        }


        //TODO: this does not currently desegment output (or the underlying code doesn't), but it's not used anywhere
        internal void PreTranslateBatch(List<string> input, IsoLanguage sourceLang, IsoLanguage targetLang, string modelTag)
        {
            this.BatchTranslationOngoing = true;

            var mtModel = this.SelectModel(sourceLang, targetLang, modelTag, true);

            Log.Information($"Pretranslating a batch of translations with model tag {modelTag}, model {mtModel.Name} will be used.");
            if (mtModel.Status == MTModelStatus.OK)
            {
                var batchProcess = mtModel.PreTranslateBatch(input, sourceLang, targetLang);
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

        private MTModel GetModelByTag(string tag, IsoLanguage srcLang, IsoLanguage trgLang, bool includeIncomplete = false)
        {
            //There could be multiple models finetuned with the same tag, use the latest.
            if (includeIncomplete)
            {
                return this.LocalModels.FirstOrDefault(
                x =>
                    x.ModelConfig.ModelTags.Contains(tag) &&
                    x.SourceLanguages.Any(y => y.IsCompatibleLanguage(srcLang)) &&
                    x.TargetLanguages.Any(y => y.IsCompatibleLanguage(trgLang)));
            }
            else
            {
                return this.LocalModels.FirstOrDefault(
                x =>
                    x.ModelConfig.ModelTags.Contains(tag) &&
                    x.SourceLanguages.Any(y => y.IsCompatibleLanguage(srcLang)) &&
                    x.TargetLanguages.Any(y => y.IsCompatibleLanguage(trgLang)) &&
                    x.Status == MTModelStatus.OK);
            }


        }

        private void modelListDownloadComplete(Uri nextUri, List<string> filePaths, object sender, DownloadStringCompletedEventArgs e)
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
            var newFilePaths = bucket.Descendants(ns + "Key").Select(x => x.Value);

            filePaths.AddRange(newFilePaths);

            var nextMarker = bucket.Descendants(ns + "NextMarker").SingleOrDefault();
            if (nextMarker != null)
            {
                using (var client = new WebClient())
                {
                    Log.Information($"Fetching page {nextUri}");
                    client.DownloadStringCompleted += (x, y) => modelListDownloadComplete(nextUri, filePaths, x, y);
                    var uriBuilder = new UriBuilder(nextUri);
                    var query = HttpUtility.ParseQueryString(uriBuilder.Query);
                    query["marker"] = nextMarker.Value;
                    uriBuilder.Query = query.ToString();
                    client.DownloadStringAsync(uriBuilder.Uri);
                }
            }
            else
            {
                AddOnlineStorageModels(filePaths, nextUri);

                //all online models have been loaded, check which ones have already been installed
                //and mark accordingly
                foreach (var onlineModel in this.onlineModels.ToList())
                {
                    this.CheckIfOnlineModelInstalled(onlineModel);
                }

                this.FilterOnlineModels();
            }
        }

        private void CheckIfOnlineModelInstalled(MTModel onlineModel)
        {
            var localModelPaths = this.LocalModels.Select(x => x.ModelPath.Replace("\\", "/"));

            if (localModelPaths.Contains(onlineModel.ModelPath))
            {
                onlineModel.InstallStatus = "Installed";
                onlineModel.InstallProgress = 100;
            }
        }

        private void AddOnlineStorageModels(List<string> filePaths, Uri storageUri)
        {
            //Get the model zips
            var modelPaths = filePaths.Where(x => Regex.IsMatch(x, @"[^/-]+-[^/-]+/[^.]+\.zip"));
            var yamlHash = filePaths.Where(x => Regex.IsMatch(x, @"[^/-]+-[^/-]+/[^.]+\.yml")).ToHashSet();

            foreach (var modelPath in modelPaths)
            {
                var modelUri = new Uri(storageUri, modelPath);
                var yaml = Regex.Replace(modelPath, @"\.zip$", ".yml");

                //OPUS-MT-models yml files have a different format, and the necessary data
                //is shown in the model path (source and language files), so only fetch yaml
                //for Tatoeba models (although currently model info for Tatoeba is fetched from
                //a single file, so this never fires now, but save in case). 
                if (yamlHash.Contains(yaml) && storageUri.Segments[1].Contains("Tatoeba-MT"))
                {
                    using (var client = new WebClient())
                    {
                        this.yamlDownloads.Add(modelUri);
                        client.DownloadStringCompleted +=
                            (x, y) => modelYamlDownloadComplete(modelUri, modelPath, x, y);
                        var yamlUri = new Uri(storageUri, yaml);
                        client.DownloadStringAsync(yamlUri);
                    }
                }
                else
                {
                    //Tatoeba models should all have yaml metadata
                    if (storageUri.Segments[1].Contains("Tatoeba-MT"))
                    {
                        Log.Error($"Model {modelUri} has no corresponding yaml file in storage, model will not be added to list of online models.");
                    }
                    //OPUS-MT models have no yaml files, so add them as they are
                    else
                    {
                        var model = new MTModel(modelPath.Replace(".zip", ""), modelUri);
                        //OPUS-MT models are all normal transformers (most are transformer-align, but
                        //use just "transformer"). The model type is used to exclude transformer-big,
                        //so it doesn't matter much whether the other types are 100% correct
                        model.ModelType = "transformer";
                        this.onlineModels.Add(model);
                    }
                }
            }

            this.FilterOnlineModels();
        }

        private void modelYamlDownloadComplete(Uri modelUri, string model, object sender, DownloadStringCompletedEventArgs e)
        {
            var yamlString = e.Result;
            yamlString = Regex.Replace(yamlString, "- (>>[^<]+<<)", "- \"$1\"");
            yamlString = Regex.Replace(yamlString, "(?<!- )'(>>[^<]+<<)'", "- \"$1\"");
            if (Regex.Match(yamlString, @"(?<!- )devset = top").Success)
            {
                Log.Information($"Corrupt yaml line in model {model} yaml file, applying fix");
                yamlString = Regex.Replace(yamlString, @"(?<!- )devset = top", "devset: top");

            }
            if (yamlString.Contains("unused dev/test data is added to training data"))
            {
                Log.Information($"Corrupt yaml line in model {model} yaml file, applying fix");
                yamlString = Regex.Replace(
                        yamlString,
                        @"unused dev/test data is added to training data",
                        "other: unused dev/test data is added to training data");
            }
            var onlineModel = new MTModel(model.Replace(".zip", ""), modelUri, yamlString);
            this.CheckIfOnlineModelInstalled(onlineModel);
            this.onlineModels.Add(onlineModel);

            this.yamlDownloads.Remove(modelUri);
            if (this.yamlDownloads.Count == 0)
            {
                this.FilterOnlineModels();
                this.OnlineModelListFetched = true;
            }
            else
            {
                this.OnlineModelListFetched = false;
            }
        }

        internal TranslationPair TranslateWithModel(string input, string modelName)
        {
            var model = this.LocalModels.Single(x => x.Name == modelName);
            //This will only work properly with monolingual models, but not sure if this method is actually used anywhere.
            return model.Translate(input, model.SourceLanguages.First(), model.TargetLanguages.First()).Result;
        }


        internal void StartCustomization(
            ParallelFilePair inputPair,
            ParallelFilePair validationPair,
            List<string> uniqueNewSegments,
            IsoLanguage srcLang,
            IsoLanguage trgLang,
            string modelTag,
            bool includePlaceholderTags,
            bool includeTagPairs,
            MTModel baseModel)
        {
            var customTask = Task.Run(() =>
            Customize(
                inputPair,
                validationPair,
                uniqueNewSegments,
                srcLang,
                trgLang,
                modelTag,
                includePlaceholderTags,
                includeTagPairs,
                baseModel));
            customTask.ContinueWith(taskExceptionHandler, TaskContinuationOptions.OnlyOnFaulted);
        }

        public void StartCustomization(
            List<ParallelSentence> input,
            List<ParallelSentence> validation,
            List<string> uniqueNewSegments,
            IsoLanguage srcLang,
            IsoLanguage trgLang,
            string modelTag,
            bool includePlaceholderTags,
            bool includeTagPairs,
            MTModel baseModel = null)
        {
            var customTask = Task.Run(() => Customize(input, validation, uniqueNewSegments, srcLang, trgLang, modelTag, includePlaceholderTags, includeTagPairs, baseModel));
            customTask.ContinueWith(taskExceptionHandler, TaskContinuationOptions.OnlyOnFaulted);
        }

        private void taskExceptionHandler(Task obj)
        {
            Log.Error($"Task failed due to the following exception: {obj.Exception}");
        }

        internal void Customize(
            List<ParallelSentence> input,
            List<ParallelSentence> validation,
            List<string> uniqueNewSegments,
            IsoLanguage srcLang,
            IsoLanguage trgLang,
            string modelTag,
            bool includePlaceholderTags,
            bool includeTagPairs,
            MTModel baseModel)
        {
            //Make sure here that there's no overlap between train and validation sets
            input.RemoveAll(x => validation.Contains(x));

            //Write the tuning set as two files
            var fileGuid = Guid.NewGuid();
            var srcFile = Path.Combine(Path.GetTempPath(), $"{fileGuid}.{srcLang.ShortestIsoCode}");
            var trgFile = Path.Combine(Path.GetTempPath(), $"{fileGuid}.{trgLang.ShortestIsoCode}");
            var validSrcFile = Path.Combine(Path.GetTempPath(), $"{fileGuid}.validation.{srcLang.ShortestIsoCode}");
            var validTrgFile = Path.Combine(Path.GetTempPath(), $"{fileGuid}.validation.{trgLang.ShortestIsoCode}");

            var inputPair = new ParallelFilePair(input, srcFile, trgFile);
            var validPair = new ParallelFilePair(validation, validSrcFile, validTrgFile);

            this.Customize(
                inputPair,
                validPair,
                uniqueNewSegments,
                srcLang,
                trgLang,
                modelTag,
                includePlaceholderTags,
                includeTagPairs,
                baseModel);
        }

        internal void Customize(
            ParallelFilePair inputPair,
            ParallelFilePair validationPair,
            List<string> uniqueNewSegments,
            IsoLanguage srcLang,
            IsoLanguage trgLang,
            string modelTag,
            bool includePlaceholderTags,
            bool includeTagPairs,
            MTModel baseModel)
        {
            modelTag = Regex.Replace(modelTag, @"[^\w-]", "_");
            modelTag = String.Join("", modelTag.Select(x => x < 127 ? x : '_'));

            if (baseModel == null)
            {
                baseModel = this.GetPrimaryModel(srcLang, trgLang);
            }

            //Select a custom dir that doesn't exist
            DirectoryInfo customDir = new DirectoryInfo($"{baseModel.InstallDir}_{modelTag}");
            var nameIndex = 0;
            while (customDir.Exists)
            {
                nameIndex++;
                customDir = new DirectoryInfo($"{baseModel.InstallDir}_{modelTag}_{nameIndex}");
            }

            Log.Information($"Fine-tuning a new model with model tag {modelTag} from base model {baseModel.Name}.");
            //Add an entry for an incomplete model to the model list
            var modelPath = Regex.Match(customDir.FullName, @"[^\\]+\\[^\\]+$").Value;
            var incompleteModel = new MTModel(
                    $"{baseModel.Name}_{modelTag}",
                    modelPath,
                    baseModel.SourceLanguages,
                    baseModel.TargetLanguages,
                    MTModelStatus.Finetuning,
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
                uniqueNewSegments,
                srcLang,
                trgLang
                );

            customizer.ProgressChanged += incompleteModel.CustomizationProgressHandler;
            customizer.ProcessExited += incompleteModel.ExitHandler;

            var trainProcess = customizer.Customize();

            //Add process to model and save its config (the directory exists at this point, 
            //so config can be saved).
            incompleteModel.FinetuneProcess = trainProcess;
            incompleteModel.SaveModelConfig();
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
                try
                {
                    this.LocalModels.Add(
                        new MTModel(
                            Regex.Match(modelPath, @"[^\\]+\\[^\\]+$").Value,
                            modelPath,
                            this.AutoPreEditRuleCollections,
                            this.AutoPostEditRuleCollections));
                }
                catch
                {
                    Log.Error($"Model path invalid: {modelPath}.");
                }
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

        public Task<TranslationPair> Translate(string input, IsoLanguage srcLang, IsoLanguage trgLang, string modelTag)
        {
            
            var mtModel = this.SelectModel(srcLang, trgLang, modelTag);

            if (mtModel == null)
            {
                if (App.Overlay != null)
                {
                    Application.Current.Dispatcher.Invoke(() => App.Overlay.ClearTranslation());
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        App.Overlay.ShowMessageInOverlay(
                            "No languages specified, and no override model selected. If using the Chrome addin, make sure to set an override model.");
                    });
                }
                throw new FaultException($"No MT model available for {srcLang}-{trgLang}");
            }

            //If override model has been selected, switch target language to the
            //target language chosen in the UI (this makes it possible to use multilingual
            //models as override models).
            if (this.OverrideModel != null)
            {
                trgLang = this.OverrideModelTargetLanguage;
            }

            var translationTask = mtModel.Translate(input, srcLang, trgLang);

            if (App.Overlay != null)
            {
                Application.Current.Dispatcher.Invoke(() => App.Overlay.ClearTranslation());
                translationTask.ContinueWith(x => Application.Current.Dispatcher.Invoke(() =>
                    {
                        App.Overlay.UpdateTranslation(x.Result);
                    }));
            }
            return translationTask;
        }

        private MTModel GetPrimaryModel(IsoLanguage srcLang, IsoLanguage trgLang)
        {

            var languagePairModels =
                this.LocalModels.Where(x =>
                    x.SourceLanguages.Any(y => y.IsCompatibleLanguage(srcLang)) &&
                    x.TargetLanguages.Any(y => y.IsCompatibleLanguage(trgLang)));
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
                    //Pick from models that are not fine-tuned
                    var nonFinetuned = languagePairModels.Where(x => !x.ModelConfig.Finetuned);
                    if (nonFinetuned.Any())
                    {
                        //Prefer bilingual models
                        var bilingual = nonFinetuned.Where(x => !x.IsMultilingualModel);
                        if (bilingual.Any())
                        {
                            primaryModel = bilingual.First();
                        }
                        else
                        {
                            primaryModel = nonFinetuned.First();
                        }
                    }
                    else
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

        public List<string> GetAllLanguagePairs()
        {
            var languagePairs = new List<string>();
            foreach (var model in this.LocalModels)
            {
                var modelLanguagePairs = from sourceLang in model.SourceLanguages
                                         from targetLang in model.TargetLanguages
                                         select $"{sourceLang}-{targetLang}";
                languagePairs.AddRange(modelLanguagePairs);
            }

            return languagePairs.Distinct().ToList();
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
            Uri modelUri,
            string modelName,
            DownloadProgressChangedEventHandler wc_DownloadProgressChanged,
            AsyncCompletedEventHandler wc_DownloadComplete)
        {
            var downloadPath = Path.Combine(this.OpusModelDir.FullName, modelName + ".zip");
            try
            {
                using (var client = new WebClient())
                {
                    client.DownloadProgressChanged += wc_DownloadProgressChanged;
                    client.DownloadFileCompleted += wc_DownloadComplete;
                    Directory.CreateDirectory(Path.GetDirectoryName(downloadPath));
                    client.DownloadFileAsync(modelUri, downloadPath);
                }

            }
            catch (Exception ex)
            {
                Log.Error($"Model download failed: {ex.Message}");
            }
}

        //This is used with automatic downloads from the object storage, where the
        //language pair is contained in the object storage path
        internal string ExtractModel(string modelPath, bool deleteZip = false)
        {
            string modelFolder = Path.Combine(this.OpusModelDir.FullName, modelPath);
            string zipPath = modelFolder + ".zip";

            ZipFile.ExtractToDirectory(zipPath, modelFolder);
            if (deleteZip)
            {
                File.Delete(zipPath);
            }
            return modelFolder;
        }

        //This is used for extraction from a local zip, where the language pair needs to
        //be extracted from the README.md file (some nicer metadata file might be useful)
        internal void ExtractModel(FileInfo zipFile)
        {
            var tempExtractionPath = Path.Combine(Path.Combine(Path.GetTempPath(),"opus_extract"), zipFile.Name);
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


    }
}
