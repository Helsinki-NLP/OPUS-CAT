using Sdl.ProjectAutomation.AutomaticTasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sdl.FileTypeSupport.Framework.IntegrationApi;
using Sdl.ProjectAutomation.Core;
using Sdl.LanguagePlatform.TranslationMemoryApi;
using System.Net;
using Sdl.Core.Globalization;
using System.Windows.Forms;
using Sdl.Core.Settings;
using Sdl.LanguagePlatform.TranslationMemory;

namespace FiskmoTranslationProvider
{
     
    [AutomaticTask("FiskmoBatchTask",
                   "Fiskmo fine-tune and translate",
                   "Task for fine-tuning Fiskmo models with project data, also support batch translation. IMPORTANT: Segment the files before running this task by opening them in the editor and saving, or by running Pretranslate or Pseudotranslate tasks.",
                   GeneratedFileType = AutomaticTaskFileType.None)]
    //[TODO] You can change the file type according to your needs
    [AutomaticTaskSupportedFileType(AutomaticTaskFileType.BilingualTarget)]
    [RequiresSettings(typeof(FinetuneBatchTaskSettings), typeof(FinetuneBatchTaskSettingsPage))]
    
    class FinetuneBatchTask : AbstractFileContentProcessingAutomaticTask
    {
        private FinetuneBatchTaskSettings settings;
        private FiskmoOptions fiskmoOptions;
        private Dictionary<Language, List<ITranslationMemoryLanguageDirection>> tms;
        internal Dictionary<Language,List<Tuple<string, string>>> ProjectTranslations;

        public Dictionary<Language, List<string>> ProjectNewSegments { get; private set; }

        private FiskmoProviderElementVisitor sourceVisitor;
        private FiskmoProviderElementVisitor targetVisitor;

        public Dictionary<Language,List<TranslationUnit>> ProjectFuzzies { get; private set; }

        //Keep track of sentences collected. Stop fuzzy collecting once max limit is reached to
        //prevent slowdown (fuzzies are secondary, exact match translation pairs are still collected).
        private int collectedSentencePairCount;

        protected override void OnInitializeTask()
        {
            this.collectedSentencePairCount = 0;
            this.settings = GetSetting<FinetuneBatchTaskSettings>();
            this.fiskmoOptions = new FiskmoOptions(new Uri(this.settings.ProviderOptions));

            //Use project guid in case no model tag specified
            if (this.fiskmoOptions.modelTag == "")
            {
                this.fiskmoOptions.modelTag = this.Project.GetProjectInfo().Id.ToString();
            }

            //Get instances of the translation memories included in the project.
            this.tms = this.InstantiateProjectTms();

            this.ProjectTranslations = new Dictionary<Language, List<Tuple<string, string>>>();
            this.ProjectNewSegments = new Dictionary<Language, List<string>>();
            this.ProjectFuzzies = new Dictionary<Language, List<TranslationUnit>>();
            this.sourceVisitor = new FiskmoProviderElementVisitor();
            this.targetVisitor = new FiskmoProviderElementVisitor();
            base.OnInitializeTask();
        }

        protected override void ConfigureConverter(ProjectFile projectFile, IMultiFileConverter multiFileConverter)
        {
            //This collects existing translations and segments without full matches from the project files
            var languageDirection = projectFile.GetLanguageDirection();
            var targetLanguage = languageDirection.TargetLanguage;
            FileReader _task = new FileReader(settings);
            multiFileConverter.AddBilingualProcessor(_task);
            multiFileConverter.Parse();

            var targetLang = projectFile.GetLanguageDirection().TargetLanguage;
            if (this.ProjectTranslations.ContainsKey(targetLang))
            {
                this.ProjectTranslations[targetLang].AddRange(_task.FileTranslations);
                this.ProjectNewSegments[targetLang].AddRange(_task.FileNewSegments);
            }
            else
            {
                this.ProjectTranslations[targetLang] = _task.FileTranslations;
                this.ProjectNewSegments[targetLang] = _task.FileNewSegments;
            }
        }

        
        /// <summary>
        /// Instantiate all file TMs for each language pair
        /// </summary>
        /// <returns></returns>
        private Dictionary<Language,List<ITranslationMemoryLanguageDirection>> InstantiateProjectTms()
        {
            var tms = new Dictionary<Language, List<ITranslationMemoryLanguageDirection>>();
            foreach (var lang in this.Project.GetProjectInfo().TargetLanguages)
            {
                var tpConfig = this.Project.GetTranslationProviderConfiguration(lang);
                
                if (!tpConfig.OverrideParent)
                {
                    tpConfig = this.Project.GetTranslationProviderConfiguration();
                }
                List<ITranslationMemoryLanguageDirection> tps = new List<ITranslationMemoryLanguageDirection>();
                foreach (var entry in tpConfig.Entries)
                {
                    var uri = entry.MainTranslationProvider.Uri;
                    //only instantiate file TMs, users can use server tms by creating project TMs if neceessary
                    //handling TM creds is too involved.
                    if (uri.Scheme.Contains("sdltm.file"))
                    {
                        var factory = TranslationProviderManager.GetTranslationProviderFactory(uri);
                        var tp = factory.CreateTranslationProvider(uri, entry.MainTranslationProvider.State, null);
                        if (tp.TranslationMethod == TranslationMethod.TranslationMemory)
                        {
                            FileBasedTranslationMemory tm = (FileBasedTranslationMemory)tp;
                            var langDirs = 
                                tm.SupportedLanguageDirections.Where(
                                    x => x.TargetCulture.ToString() == lang.CultureInfo.ToString());

                            foreach (var langDir in langDirs)
                            {
                                tps.Add(tm.GetLanguageDirection(langDir));
                            }
                        }
                    }
                }

                tms.Add(lang, tps);
            }
            
            return tms;
        }

        private void AddFiskmoProviderToProject()
        {
            //Add Fiskmö MT provider to the project
            var tpConfig = this.Project.GetTranslationProviderConfiguration();

            //Don't add another Fiskmo provider
            if (!tpConfig.Entries.Any(x => x.MainTranslationProvider.Uri.Scheme == FiskmoProvider.FiskmoTranslationProviderScheme))
            {
                var fiskmoRef = new TranslationProviderReference(fiskmoOptions.Uri);
                tpConfig.Entries.Add(
                    new TranslationProviderCascadeEntry(fiskmoRef, false, true, false));

                this.Project.UpdateTranslationProviderConfiguration(tpConfig);
            }
        }

        private void BatchTranslate()
        {
            var projectInfo = this.Project.GetProjectInfo();
            var projectGuid = projectInfo.Id;
            var sourceCode = projectInfo.SourceLanguage.CultureInfo.TwoLetterISOLanguageName;

            foreach (var targetLang in projectInfo.TargetLanguages)
            {
                var targetCode = targetLang.CultureInfo.TwoLetterISOLanguageName;
                var uniqueNewSegments = this.ProjectNewSegments[targetLang].Distinct().ToList();
                //Send the new segments to MT service
                var result = FiskmöMTServiceHelper.PreTranslateBatch(fiskmoOptions.mtServiceAddress, fiskmoOptions.mtServicePort, uniqueNewSegments, sourceCode, targetCode, fiskmoOptions.modelTag);

                switch (result)
                {
                    case "batch translation or customization already in process":
                        throw new Exception("MT engine is currently batch translating or fine-tuning, wait for previous job to finish (or cancel it by restarting MT engine).");
                    default:
                        break;
                }
            }
        }

        private void Finetune()
        {
            var projectInfo = this.Project.GetProjectInfo();
            var projectGuid = projectInfo.Id;
            var sourceCode = projectInfo.SourceLanguage.CultureInfo.TwoLetterISOLanguageName;

            foreach (var targetLang in projectInfo.TargetLanguages)
            {
                var targetCode = targetLang.CultureInfo.TwoLetterISOLanguageName;

                //Remove duplicates
                var uniqueProjectTranslations = this.ProjectTranslations[targetLang].Distinct().ToList();
                List<string> uniqueNewSegments = this.ProjectNewSegments[targetLang].Distinct().ToList();

                var transUnitExtractor =
                    new FinetuneTransUnitExtractor(
                        this.tms[targetLang],
                        uniqueNewSegments,
                        new List<int>() { 100, 90, 80, 70, 60, 50 },
                        settings.MaxFinetuningSentences,
                        settings.ConcordanceMaxResults,
                        settings.FuzzyMaxResults,
                        settings.MaxConcordanceWindow);
                
                transUnitExtractor.Extract(
                    this.settings.ExtractFuzzies,
                    this.settings.ExtractConcordanceMatches,
                    this.settings.ExtractFillerUnits);

                var finetuneSet = uniqueProjectTranslations.Union(transUnitExtractor.AllExtractedTranslations).ToList();

                if (finetuneSet.Count() < FiskmoTpSettings.Default.FinetuningMinSentencePairs)
                {
                    throw new Exception(
                        $"Not enough sentence pairs for fine-tuning. Found {finetuneSet.Count}, minimum is {FiskmoTpSettings.Default.FinetuningMinSentencePairs}");
                }

                //Send the tuning set to MT service
                var result = FiskmöMTServiceHelper.Customize(
                    this.fiskmoOptions.mtServiceAddress,
                    this.fiskmoOptions.mtServicePort,
                    finetuneSet,
                    uniqueNewSegments,
                    sourceCode,
                    targetCode,
                    this.fiskmoOptions.modelTag,
                    this.settings.IncludePlaceholderTags,
                    this.settings.IncludeTagPairs);

                switch (result)
                {
                    case "fine-tuning already in process":
                        throw new Exception("MT engine is currently batch translating or fine-tuning, wait for previous job to finish (or cancel it by restarting MT engine).");
                    default:
                        break;
                }
            }
        }

        public override void TaskComplete()
        {
            var projectInfo = this.Project.GetProjectInfo();
            var projectGuid = projectInfo.Id;
            var sourceCode = projectInfo.SourceLanguage.CultureInfo.TwoLetterISOLanguageName;
            
            if (settings.AddFiskmoProvider)
            {
                this.AddFiskmoProviderToProject();   
            }

            if (settings.Finetune)
            {
                this.Finetune();   
            }

            //Send the new segments to MT engine for pretranslation.
            //If finetuning is selected, the new segments are translated after
            //customization finished, so this is only for BatchTranslateOnly
            if (settings.BatchTranslate == true && settings.Finetune == false)
            {
                this.BatchTranslate();    
            }
        }
    
    }
}
