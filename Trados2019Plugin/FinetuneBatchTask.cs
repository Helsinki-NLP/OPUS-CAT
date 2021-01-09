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

namespace OpusCatTranslationProvider
{
     
    [AutomaticTask("OPUSCATBatchTask",
                   "OPUS-CAT Finetune",
                   "Task for finetuning OPUS MT models. IMPORTANT: This task works only on segmented files. If files are not segmented, segment them by opening them in the editor and saving, or by running Pretranslate or Pseudotranslate tasks.",
                   GeneratedFileType = AutomaticTaskFileType.None)]
    //[TODO] You can change the file type according to your needs
    [AutomaticTaskSupportedFileType(AutomaticTaskFileType.BilingualTarget)]
    [RequiresSettings(typeof(FinetuneBatchTaskSettings), typeof(FinetuneBatchTaskSettingsPage))]
    
    class FinetuneBatchTask : AbstractFileContentProcessingAutomaticTask
    {
        private FinetuneBatchTaskSettings settings;
        private OpusCatOptions opusCatOptions;
        private Dictionary<Language, List<ITranslationMemoryLanguageDirection>> tms;
        internal Dictionary<Language,List<Tuple<string, string>>> ProjectTranslations;

        public Dictionary<Language, List<string>> ProjectNewSegments { get; private set; }

        private OpusCatProviderElementVisitor sourceVisitor;
        private OpusCatProviderElementVisitor targetVisitor;

        public Dictionary<Language,List<TranslationUnit>> ProjectFuzzies { get; private set; }

        //Keep track of sentences collected. Stop fuzzy collecting once max limit is reached to
        //prevent slowdown (fuzzies are secondary, exact match translation pairs are still collected).
        private int collectedSentencePairCount;

        protected override void OnInitializeTask()
        {
            var fileLanguageDirections = TaskFiles.Select(x => x.GetLanguageDirection().TargetLanguage.IsoAbbreviation).Distinct();
            if (fileLanguageDirections.Count() > 1)
            {
                throw new Exception(
                    $"This batch task can only be applied to one language direction at a time. Select only files with same language direction and try again.");
            }
            else if (fileLanguageDirections.Count() == 0)
            {
                throw new Exception(
                    $"No target files selected.");
            }
            this.collectedSentencePairCount = 0;
            this.settings = GetSetting<FinetuneBatchTaskSettings>();
            this.opusCatOptions = new OpusCatOptions(new Uri(this.settings.ProviderOptions));

            //Use project guid in case no model tag specified
            if (this.opusCatOptions.modelTag == "")
            {
                this.opusCatOptions.modelTag = this.Project.GetProjectInfo().Id.ToString();
            }

            //Get instances of the translation memories included in the project.
            this.tms = this.InstantiateProjectTms();

            this.ProjectTranslations = new Dictionary<Language, List<Tuple<string, string>>>();
            this.ProjectNewSegments = new Dictionary<Language, List<string>>();
            this.ProjectFuzzies = new Dictionary<Language, List<TranslationUnit>>();
            this.sourceVisitor = new OpusCatProviderElementVisitor();
            this.targetVisitor = new OpusCatProviderElementVisitor();
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

        private void AddOpusCatProviderToProject()
        {
            //Add Opus CAT MT provider to the project
            var tpConfig = this.Project.GetTranslationProviderConfiguration();

            //Don't add another Opus CAT provider
            if (!tpConfig.Entries.Any(x => x.MainTranslationProvider.Uri.Scheme == OpusCatProvider.OpusCatTranslationProviderScheme))
            {
                var opusCatRef = new TranslationProviderReference(opusCatOptions.Uri);
                tpConfig.Entries.Add(
                    new TranslationProviderCascadeEntry(opusCatRef, false, true, false));

                this.Project.UpdateTranslationProviderConfiguration(tpConfig);
            }
        }

        private void PreOrderMt()
        {
            var projectInfo = this.Project.GetProjectInfo();
            var projectGuid = projectInfo.Id;
            var sourceCode = projectInfo.SourceLanguage.CultureInfo.TwoLetterISOLanguageName;

            foreach (var targetLang in projectInfo.TargetLanguages)
            {
                var targetCode = targetLang.CultureInfo.TwoLetterISOLanguageName;
                var uniqueNewSegments = this.ProjectNewSegments[targetLang].Distinct().ToList();
                //Send the new segments to MT service
                var result = OpusCatMTServiceHelper.PreOrderBatch(opusCatOptions.mtServiceAddress, opusCatOptions.mtServicePort, uniqueNewSegments, sourceCode, targetCode, opusCatOptions.modelTag);

                switch (result)
                {
                    case "batch translation or customization already in process":
                        throw new Exception("MT engine is currently batch translating or fine-tuning, wait for previous job to finish (or cancel it by restarting MT engine).");
                    default:
                        break;
                }
            }
        }

        private List<Tuple<string, string>> ExtractFromTm(
            List<ITranslationMemoryLanguageDirection> tms,
            List<string> uniqueNewSegments)
        {

            //assign fuzzy min and all above percentage divisible by ten as fuzzybands
            var fuzzyBands = Enumerable.Range(settings.FuzzyMinPercentage, 100).Where(
                x => (x % 10 == 0 && x <= 100) || x == settings.FuzzyMinPercentage).ToList();

            var transUnitExtractor =
                new FinetuneTransUnitExtractor(
                    tms,
                    uniqueNewSegments,
                    fuzzyBands,
                    this.settings.MaxFinetuningSentences,
                    this.settings.ConcordanceMaxResults,
                    this.settings.FuzzyMaxResults,
                    this.settings.MaxConcordanceWindow);

            transUnitExtractor.Extract(
                this.settings.ExtractFuzzies,
                this.settings.ExtractConcordanceMatches,
                this.settings.ExtractFillerUnits);

            return transUnitExtractor.AllExtractedTranslations;
        }

        private void Finetune()
        {
            var projectInfo = this.Project.GetProjectInfo();
            var projectGuid = projectInfo.Id;
            var sourceCode = projectInfo.SourceLanguage.CultureInfo.TwoLetterISOLanguageName;
            var collectedLanguages = this.ProjectNewSegments.Keys.Union(this.ProjectTranslations.Keys);
            
            if (ConnectionControl.MtServiceLanguagePairs == null)
            {
                throw new Exception($"Language pair data not available, check if connection with OPUS-CAT MT Engine is working.");
            }
            
            var languagePairsWithMt = collectedLanguages.Where(x => ConnectionControl.MtServiceLanguagePairs.Contains($"{sourceCode}-{x.IsoAbbreviation}"));

            //Select the target language with most segments as the one to finetune.
            //If there are many, the selection will be random I suppose.
            Language primaryTargetLanguage;
            if (languagePairsWithMt.Any())
            {
                primaryTargetLanguage = languagePairsWithMt.OrderByDescending(x => this.ProjectTranslations[x].Count + this.ProjectNewSegments.Count).First();
            }
            else
            {
                //This is a backoff in case the iso code of the language does not match
                //the language pair codes from the mt service (e.g. with rare languages where
                //codes may be strange).
                primaryTargetLanguage = collectedLanguages.OrderByDescending(x => this.ProjectTranslations[x].Count + this.ProjectNewSegments.Count).First();
            }

            var targetCode = primaryTargetLanguage.CultureInfo.TwoLetterISOLanguageName;
            
            //Remove duplicates
            var uniqueProjectTranslations = this.ProjectTranslations[primaryTargetLanguage].Distinct().ToList();
            List<string> uniqueNewSegments = this.ProjectNewSegments[primaryTargetLanguage].Distinct().ToList();

            List<Tuple<string, string>> finetuneSet;
            if (this.tms[primaryTargetLanguage].Any())
            {
                var tmExtracts = this.ExtractFromTm(this.tms[primaryTargetLanguage], uniqueNewSegments);
                finetuneSet = uniqueProjectTranslations.Union(tmExtracts).ToList();
            }
            else
            {
                finetuneSet = uniqueProjectTranslations;
            }

                

            if (finetuneSet.Count() < OpusCatTpSettings.Default.FinetuningMinSentencePairs)
            {
                throw new Exception(
                    $"Not enough sentence pairs for fine-tuning. Found {finetuneSet.Count}, minimum is {OpusCatTpSettings.Default.FinetuningMinSentencePairs}");
            }

            //Send the tuning set to MT service
            var result = OpusCatMTServiceHelper.Customize(
                this.opusCatOptions.mtServiceAddress,
                this.opusCatOptions.mtServicePort,
                finetuneSet,
                uniqueNewSegments,
                sourceCode,
                targetCode,
                this.opusCatOptions.modelTag,
                this.settings.IncludePlaceholderTags,
                this.settings.IncludeTagPairs);
            
        }

        public override void TaskComplete()
        {
            var projectInfo = this.Project.GetProjectInfo();
            var projectGuid = projectInfo.Id;
            var sourceCode = projectInfo.SourceLanguage.CultureInfo.TwoLetterISOLanguageName;
            
            if (settings.AddOpusCatProvider)
            {
                this.AddOpusCatProviderToProject();   
            }

            if (settings.Finetune)
            {
                this.Finetune();   
            }

            //Send the new segments to MT engine for pretranslation.
            //If finetuning is selected, the new segments are translated after
            //customization finished, so this is only for BatchTranslateOnly
            if (settings.PreOrderMtForNewSegments == true && settings.Finetune == false)
            {
                this.PreOrderMt();    
            }
        }
    
    }
}
