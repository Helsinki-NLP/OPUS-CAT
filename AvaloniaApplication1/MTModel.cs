using Avalonia.Data.Converters;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia;
using Python.Runtime;
using Serilog;
using System;
using System.Collections;
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
using YamlDotNet.Serialization;

namespace OpusCatMtEngine
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
        private dynamic lemmatizer;

        internal List<Tuple<int, int, string>> Lemmatize(string input)
        {
            List<Tuple<int, int, string>> lemmaList = new List<Tuple<int, int, string>>();
            using (Py.GIL())
            {
                var output = new List<Tuple<int, int, string>>();

                dynamic processed = this.lemmatizer(input);

                foreach (var sentence in processed.sentences)
                {
                    foreach (var word in sentence.words)
                    {
                        output.Add(new Tuple<int, int, string>((int)word.start_char, (int)word.end_char, (string)word.lemma));
                    }
                }

                return output;
            }

        }

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
                    //this.marianProcesses[langpair] = null;
                }
            }
            this.marianProcesses = null;
        }

        private Dictionary<int, List<TermMatch>> GetTermMatches(string input)
        {
            var termMatches = new Dictionary<int, List<TermMatch>>();

            foreach (var term in this.Terminology.Terms)
            {
                var thisTermMatches = term.SourcePatternRegex.Matches(input);
                foreach (Match termMatch in thisTermMatches)
                {
                    if (termMatches.ContainsKey(termMatch.Index))
                    {
                        termMatches[termMatch.Index].Add(
                            new TermMatch(term, termMatch));
                    }
                    else
                    {
                        termMatches[termMatch.Index] = new List<TermMatch>() {
                            new TermMatch(term, termMatch)};
                    }
                }
                
            }

            return termMatches;
        }


        private Dictionary<int,List<TermMatch>> GetLemmaMatches(string input, Dictionary<int, List<TermMatch>> termMatches = null)
        {
            if (termMatches == null)
            {
                termMatches = new Dictionary<int, List<TermMatch>>();
            }
            

            //Get lemmatized input and find lemmatized term matches. Prioritize normal term matches
            //in case of overlap
            //var lemmatizedInput = PythonNetHelper.Lemmatize(this.sourceLanguages.First().ShortestIsoCode, input);
            var lemmatizedInput = this.Lemmatize(input);

            var lemmaToPositionDict = new Dictionary<string, List<int>>();
            int lemmaCounter = 0;
            foreach (var lemma in lemmatizedInput.Select(x => x.Item3))
            {
                if (lemmaToPositionDict.ContainsKey(lemma))
                {
                    lemmaToPositionDict[lemma].Add(lemmaCounter);
                }
                else
                {
                    lemmaToPositionDict[lemma] = new List<int>() { lemmaCounter };
                }
                lemmaCounter++;
            }

            foreach (var term in this.Terminology.Terms.Where(x => x.MatchSourceLemma))
            {

                if (term.SourceLemmas == null)
                {
                    term.SourceLemmas = this.Lemmatize(term.SourcePattern).Select(x => x.Item3).ToList();
                }
                var sourceLemmas = term.SourceLemmas;

                //Check if first lemma in term found in sentence

                bool termLemmaFound;

                if (lemmaToPositionDict.ContainsKey(sourceLemmas[0]))
                {
                    var firstLemmaPositions = lemmaToPositionDict[sourceLemmas[0]];

                    //Then check if the other lemmas of the term follow in the source sentence
                    foreach (var startPos in firstLemmaPositions)
                    {
                        int sourceSentencePos = startPos;
                        termLemmaFound = true;
                        //start looking at second lemma
                        for (var termLemmaIndex = 1; termLemmaIndex < sourceLemmas.Count; termLemmaIndex++)
                        {
                            sourceSentencePos = startPos + termLemmaIndex;
                            if (sourceSentencePos < lemmatizedInput.Count)
                            {
                                if (lemmatizedInput[sourceSentencePos].Item3 != sourceLemmas[termLemmaIndex])
                                {
                                    termLemmaFound = false;
                                    break;
                                }
                            }
                            else
                            {
                                termLemmaFound = false;
                                break;
                            }
                        }

                        if (termLemmaFound)
                        {
                            var startChar = lemmatizedInput[startPos].Item1;
                            var endChar = lemmatizedInput[sourceSentencePos].Item2;
                            if (termMatches.ContainsKey(startChar))
                            {
                                termMatches[startChar].Add(
                                    new TermMatch(term, startChar, endChar - startChar, true));
                            }
                            else
                            {
                                termMatches[startChar] = new List<TermMatch>() {
                                            new TermMatch(term, startChar, endChar-startChar, true)};
                            }
                        }
                    }
                }
            }

            return termMatches;
        }

        public Task<TranslationPair> Translate(
            string input, 
            IsoLanguage sourceLang, 
            IsoLanguage targetLang,
            bool applyEditRules=true,
            bool applyTerminology=true)
        {
            if (String.IsNullOrWhiteSpace(input))
            {
                var translationPair = new TranslationPair(input, input, input, "0-0", this.ModelSegmentationMethod, targetLang.OriginalCode);
                return Task.FromResult<TranslationPair>(translationPair);
            }

            //Preprocess input with pre-edit rules
            if (applyEditRules)
            {
                foreach (var preEditRuleCollection in this.AutoPreEditRuleCollections)
                {
                    input = preEditRuleCollection.ProcessPreEditRules(input).Result;
                }
            }
            
            if (this.SupportsTerminology && applyTerminology)
            {
                
                //Apply terminology
                //Use a simple method of removing overlapping matches of different terms:
                //For each position record only the longest term match, then when annotating term data,
                //start from the term closest to edge and skip overlapping terms.
                var termMatches = this.GetTermMatches(input);

                //Match term at lemma level, if specified
                if (this.Terminology.Terms.Any(x => x.MatchSourceLemma))
                {
                    termMatches = this.GetLemmaMatches(input, termMatches);
                }

                int lastEditStart = input.Length;
                foreach (var index in termMatches.Keys.ToList().OrderByDescending(x => x))
                {
                    //Start from longest match
                    var matchesDescending = termMatches[index].OrderByDescending(x => x.Length);
                    foreach (var match in matchesDescending)
                    {
                        if (match.Length + index <= lastEditStart)
                        {
                            input = input.Remove(index, match.Length).Insert(index,
                                $" <term_start> <term_mask> <term_end> {match.Term.TargetLemma} <trans_end>");
                            lastEditStart = index;
                            continue;
                        }
                    }
                }
            }
            
            //Need to get the original codes, since those are the ones the marian model uses
            var modelSourceLang =
                this.SourceLanguages.SingleOrDefault(x => x.ShortestIsoCode == sourceLang.ShortestIsoCode);
            string modelOrigSourceCode;
            if (modelSourceLang == null)
            {
                modelOrigSourceCode = this.SourceLanguages.First().OriginalCode;
            }
            else
            {
                modelOrigSourceCode = modelSourceLang.OriginalCode;
            }

            //Try to get an exact match first, then try for shortest code match, this is for
            //matching correct script variants of same languages
            var modelTargetLang =
                this.TargetLanguages.SingleOrDefault(x => x.OriginalCode == targetLang.OriginalCode);
            if (modelTargetLang == null)
            {
                modelTargetLang = this.TargetLanguages.Where(x => x.ShortestIsoCode == targetLang.ShortestIsoCode).First();
            }
            string modelOrigTargetCode;
            if (modelTargetLang == null)
            {
                modelOrigTargetCode = this.TargetLanguages.First().OriginalCode;
            }
            else
            {
                modelOrigTargetCode = modelTargetLang.OriginalCode;
            }

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
                        this.modelConfig.IncludePlaceholderTags, 
                        this.modelConfig.IncludeTagPairs);
            };

            var translationTask = this.marianProcesses[modelOrigTuple].AddToTranslationQueue(input);
            if (applyEditRules)
            {
                return this.ApplyAutoPostEditRules(translationTask, input);
            }
            else
            {
                return translationTask;
            }
            
        }

        private async Task<TranslationPair> ApplyAutoPostEditRules(Task<TranslationPair> translationTask, string input)
        {
            await translationTask;
            if (translationTask.IsCompleted)
            {
                var translationPair = translationTask.Result;
                var output = translationPair.Translation;
                //Postprocess output with post-edit rules
                foreach (var postEditRuleCollection in this.AutoPostEditRuleCollections)
                {
                    output = postEditRuleCollection.ProcessPostEditRules(input, output).Result;
                }

                if (output != translationPair.Translation)
                {
                    translationPair.AutoEditedTranslation = true;
                    translationPair.Translation = output;
                    translationPair.SegmentedTranslation = new string[] { output };
                }

                return translationPair;
            }
            else
            {
                return null;
            }
        }

        internal void DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            this.InstallProgress = e.ProgressPercentage;
        }
        
        public MTModelStatus Status { get => status; set { status = value; NotifyPropertyChanged(); } }
        
        //This creates a zip package of the model that can be moved to another computer
        internal void PackageModel()
        {
            var customModelZipDirPath = HelperFunctions.GetOpusCatDataPath(OpusCatMtEngineSettings.Default.CustomModelZipPath);
            if (!Directory.Exists(customModelZipDirPath))
            {
                Directory.CreateDirectory(customModelZipDirPath);
            }

            var zipPath = Path.Combine(customModelZipDirPath, $"{this.Name}.zip");

            if (File.Exists(zipPath))
            {
                MessageBoxManager.GetMessageBoxStandard(
                    "Model already exists",
                    $"Zipped model already exists: {zipPath}",
                    ButtonEnum.Ok);
            }

            using (var packageZip = ZipFile.Open(zipPath, ZipArchiveMode.Create))
            {

                //Include model files, README.md, spm files, tcmodel (not needed but expected), modelconfig.yml
                foreach (var modelNpzName in this.decoderSettings.models)
                {
                    //No point in compressing npz, it's already compressed
                    packageZip.CreateEntryFromFile(
                        Path.Combine(this.InstallDir, modelNpzName),
                        modelNpzName,
                        CompressionLevel.NoCompression);
    
                }



                foreach (var vocabName in decoderSettings.vocabs.Distinct())
                {
                    packageZip.CreateEntryFromFile(Path.Combine(this.InstallDir, vocabName), vocabName);
                }

                //Tatoeba models have yml configs, which may be needed for extracting info about the model
                //(especially for multilingual models).
                
                if (File.Exists(this.modelYamlFilePath))
                {
                    packageZip.CreateEntryFromFile(
                        this.modelYamlFilePath,
                        Path.GetFileName(this.modelYamlFilePath));
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
                        "LICENSE",
                        "train.log"
                    };

                foreach (var fileName in otherModelFiles)
                {
                    packageZip.CreateEntryFromFile(Path.Combine(this.InstallDir, fileName), fileName);
                }
            }

            MessageBoxManager.GetMessageBoxStandard(
                    "Model packaged",
                    "Model has been packaged and saved to {zipPath}. Click OK to go to folder.",
                    ButtonEnum.Ok);

            System.Diagnostics.Process.Start("explorer.exe", Path.GetDirectoryName(zipPath));
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
                FileInfo postCustomizationBatchFile = new FileInfo(Path.Combine(this.InstallDir, OpusCatMtEngineSettings.Default.PostFinetuneBatchName));
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

        public List<IsoLanguage> SourceLanguages
        {
            get => sourceLanguages;
            set
            {
                sourceLanguages = value;
                NotifyPropertyChanged("SourceLanguageString");
            }
        }

        public List<IsoLanguage> TargetLanguages
        {
            get => targetLanguages;
            set
            {
                targetLanguages = value;
                NotifyPropertyChanged("TargetLanguageString");
            }
        }

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
                    OpusCatMtEngineSettings.Default.TrainLogName));

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
            var deserializer = new DeserializerBuilder().IgnoreUnmatchedProperties().Build();
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
            //Recent models have yaml files containing metadata, their naming conventions may change, so
            //need to find the correct yml file. Three yml files can be excluded based on their name.
            var modelYamlCandidates = Directory.GetFiles(this.InstallDir, "*.yml").Where(
                x => !x.EndsWith("decoder.yml") && !x.EndsWith("modelconfig.yml") && !x.EndsWith("vocab.yml"));

            foreach (var candidate in modelYamlCandidates)
            {
                using (var reader = File.OpenText(candidate))
                {
                    var candidateString = reader.ReadToEnd();
                    
                    if (ParseModelYaml(candidateString))
                    {
                        this.Faulted = false;
                        this.modelYaml = candidateString;
                        this.modelYamlFilePath = candidate;
                        this.Name = Path.GetFileName(this.InstallDir);
                        break;
                    }
                    else
                    {
                        this.Faulted = true;
                    }
                }
            }
           
        }

        public MTModel(
            string modelPath, 
            string installDir,
            ObservableCollection<AutoEditRuleCollection> autoPreEditRuleCollections,
            ObservableCollection<AutoEditRuleCollection> autoPostEditRuleCollections,
            ObservableCollection<Terminology> terminologies)
        {

            this.ModelPath = modelPath;
            this.InstallDir = installDir;

            this.ParseDecoderConfig();
            this.UpdateModelYamlPath();

            this.SupportsWordAlignment = this.decoderSettings.models[0].Contains("-align");
            this.SupportsTerminology = this.decoderSettings.models[0].Contains("-terms");

            this.ParseModelConfig();

            if (this.modelYaml == null)
            {
                this.ParseModelPathForLanguages(modelPath);

                if (this.modelConfig.SourceLanguageCodes != null &&
                    this.modelConfig.TargetLanguageCodes != null)
                {
                    this.SourceLanguages = this.modelConfig.SourceLanguageCodes.Select(x => new IsoLanguage(x)).ToList();
                    this.TargetLanguages = this.modelConfig.TargetLanguageCodes.Select(x => new IsoLanguage(x)).ToList();
                }
            }
            
            this.AutoPreEditRuleCollections = new ObservableCollection<AutoEditRuleCollection>(
                this.ModelConfig.AutoPreEditRuleCollectionGuids.Select(x => autoPreEditRuleCollections.SingleOrDefault(
                    y => y.CollectionGuid == x)).Where(y => y != null));
            this.AutoPostEditRuleCollections = new ObservableCollection<AutoEditRuleCollection>(
                this.ModelConfig.AutoPostEditRuleCollectionGuids.Select(x => autoPostEditRuleCollections.SingleOrDefault(
                    y => y.CollectionGuid == x)).Where(y => y != null));

            if (this.SupportsTerminology)
            {
                this.Terminology = terminologies.SingleOrDefault(x => x.TerminologyGuid == this.ModelConfig.TerminologyGuid);
                //If there is no terminology guid or the terminology does not exits, Terminology will be null,
                //so use new Terminology
                if (this.Terminology == null)
                {
                    this.Terminology = new Terminology() { TerminologyName = $"terms for {this.Name}" };
                    this.ModelConfig.TerminologyGuid = this.Terminology.TerminologyGuid;
                    this.SaveModelConfig();
                }

                using (Py.GIL())
                {
                    dynamic stanza = Py.Import("stanza");
                    this.lemmatizer = stanza.Pipeline(
                            this.SourceLanguages[0].ShortestIsoCode, processors: "tokenize,mwt,pos,lemma");
                }

            }
            
            this.ModelConfig.ModelTags.CollectionChanged += ModelTags_CollectionChanged;
        }

        internal void SaveModelConfig()
        {
            if (this.ModelConfig == null)
            {
                this.ModelConfig = new MTModelConfig();
            }

            if (this.SourceLanguages != null && this.TargetLanguages != null)
            {
                this.ModelConfig.SourceLanguageCodes = new ObservableCollection<String>(this.SourceLanguages.Select(x => x.OriginalCode));
                this.ModelConfig.TargetLanguageCodes = new ObservableCollection<String>(this.TargetLanguages.Select(x => x.OriginalCode));
            }

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
            IsoLanguage sourceLanguage,
            IsoLanguage targetLanguage,
            Boolean preprocessedInput=false)
        {
            var batchTranslator = new MarianBatchTranslator(
                this.InstallDir,
                sourceLanguage,
                targetLanguage,
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

            string segmethodArg;
            
            switch (this.ModelSegmentationMethod)
            {
                case SegmentationMethod.Bpe:
                    segmethodArg = ".bpe";
                    break;
                case SegmentationMethod.SentencePiece:
                    segmethodArg = ".spm";
                    break;
                default:
                    return;
            }

            var evalProcess = MarianHelper.StartProcessInBackgroundWithRedirects(
                Path.Combine(OpusCatMtEngineSettings.Default.PythonDir, "python.exe"),
                $".\\Marian\\validate.py {refFile.FullName} {outOfDomainSize} {segmethodArg} {spOutput.FullName}");
        }
        

        private void ModelTags_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            this.SaveModelConfig();
        }
        
        private void ParseModelPathForLanguages(string modelPath)
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
            
            //There may be weird paths with no hyphen separating source from target languages, e.g.
            //just "westgermanic" (presumably both source and target languages are westgermanic).
            if (pathSplit[0].Contains("-"))
            {
                this.SourceLanguages =
                    pathSplit[0].Split('-')[0].Split('+').Select(x => new IsoLanguage(x)).ToList();
                this.TargetLanguages =
                    pathSplit[0].Split('-')[1].Split('+').Select(x => new IsoLanguage(x)).ToList();
            }
            
            
            this.Name = pathSplit[1];
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
            this.AutoPostEditRuleCollections = new ObservableCollection<AutoEditRuleCollection>();
            this.AutoPreEditRuleCollections = new ObservableCollection<AutoEditRuleCollection>();
            this.Terminology = new Terminology() { TerminologyName = $"terms for {this.Name}" };
            this.ModelConfig.TerminologyGuid = this.Terminology.TerminologyGuid;
            
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
                    OpusCatMtEngineSettings.Default.TrainLogName));
            }
            
            this.ModelPath = modelPath;
            this.SaveModelConfig();
        }

        //This is used for online models, model uri is included for later download of models
        public MTModel(string modelPath, Uri modelUri, string yamlString = null)
        {
            this.ModelPath = modelPath;
            this.modelYaml = yamlString;
            if (yamlString != null)
            {
                this.ParseModelYaml(this.modelYaml);
            }
            else
            {
                this.ParseModelPathForLanguages(modelPath);
            }
            this.ModelUri = modelUri;
            
        }

        private Boolean ParseModelYaml(string yamlString)
        {
            yamlString = HelperFunctions.FixOpusYaml(yamlString, this.Name);
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

                if (res.ContainsKey("modeltype"))
                {
                    this.ModelType = res["modeltype"];
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
                    this.SourceLanguages.Add(new IsoLanguage("NO SOURCE LANGUAGES"));
                    return false;
                }
                List<object> xamlTargetLangs = null;
                //There may be more target labels than target language, in case you have different
                //writing systems etc., so use target labels as target languages whenever they exist
                if (res.ContainsKey("use-target-labels"))
                {
                    xamlTargetLangs = res["use-target-labels"];
                }
                else if (res.ContainsKey("target-languages"))
                {
                    xamlTargetLangs = res["target-languages"];
                }

                if (xamlTargetLangs != null)
                {
                    foreach (var lang in xamlTargetLangs)
                    {
                        this.TargetLanguages.Add(new IsoLanguage(lang.ToString().Trim(new char[] { '>', '<' })));
                    }
                }
                else
                {
                    Log.Error($"No target langs in {this.ModelUri} yaml file.");
                    this.TargetLanguages.Add(new IsoLanguage("NO TARGET LANGUAGES"));
                    return false;
                }
            }
            catch (YamlDotNet.Core.SyntaxErrorException ex)
            {
                Log.Error($"Error in the yaml syntax of model {this.ModelUri}. Error: {ex.Message}.");
                this.SourceLanguages.Add(new IsoLanguage("ERROR IN YAML SYNTAX"));
                this.TargetLanguages.Add(new IsoLanguage("ERROR IN YAML SYNTAX"));
                return false;
            }

            return true;
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
            get
            {
                string sourceLangString = String.Join(", ", this.SourceLanguages.Select(x => x.IsoRefName));
                if (String.IsNullOrWhiteSpace(sourceLangString))
                {
                    this.Faulted = true;
                    return "No source languages available";
                }
                else
                {
                    return sourceLangString;
                }
            }
        }

        public string SourceCodesString
        {
            get { return String.Join("+", this.SourceLanguages); }
        }

        public string TargetLanguageString
        {
            get
            {
                string targetLangString = String.Join(", ", this.TargetLanguages.Select(x => x.IsoRefName));
                if (String.IsNullOrWhiteSpace(targetLangString))
                {
                    this.Faulted = true;
                    return "No target languages available";
                }
                else
                {
                    return targetLangString;
                }
            }

        }

        public string TargetCodesString
        {
            get { return String.Join("+", this.TargetLanguages); }
        }

        public string ModelPath { get; internal set; }
        public string InstallDir { get; set; }
        public bool Prioritized { get => _prioritized; set { _prioritized = value; NotifyPropertyChanged(); } }
        
        private MarianLog TrainingLog;

        public MTModelConfig ModelConfig { get => modelConfig; set => modelConfig = value; }
        public Process FinetuneProcess { get; set; }

        private string modelYaml;

        public Uri ModelUri { get; private set; }
        public bool Faulted { get; private set; }

        private MarianDecoderConfig decoderSettings;

        public bool SupportsWordAlignment { get => supportsWordAlignment; set => supportsWordAlignment = value; }
        public bool SupportsTerminology { get; private set; }
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

        private bool? hasOODValidSet;
        public bool HasOODValidSet
        {
            get
            {
                if (!hasOODValidSet.HasValue)
                {
                    var dummyOOD = Directory.GetFiles(this.InstallDir, "dummyOOD*");
                    hasOODValidSet = !dummyOOD.Any();
                }
                return hasOODValidSet.Value;
            }
        }
        
        public ObservableCollection<AutoEditRuleCollection> AutoPreEditRuleCollections
        {
            get;
            internal set;
        }

        public ObservableCollection<AutoEditRuleCollection> AutoPostEditRuleCollections
        {
            get;
            internal set;
        }

        public string ModelType
        {
            get;
            internal set;
        }

        public Terminology Terminology
        {
            get;
            internal set;
        }
        private dynamic SourceLemmatizer { get; set; }

        private MTModelStatus status;
        private MTModelConfig modelConfig;
        private FileInfo trainingLogFileInfo;
        private string modelYamlFilePath;

        //TODO: this does not currently desegment output
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
