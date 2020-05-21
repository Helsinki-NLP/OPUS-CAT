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

namespace FiskmoTranslationProvider
{
     
    [AutomaticTask("My_Custom_Batch_Task_ID",
                   "My_Custom_Batch_Task_Name",
                   "My_Custom_Batch_Task_Description",
                   //[TODO] You can change the file type according to your needs
                   GeneratedFileType = AutomaticTaskFileType.None)]
    //[TODO] You can change the file type according to your needs
    [AutomaticTaskSupportedFileType(AutomaticTaskFileType.BilingualTarget)]
    [RequiresSettings(typeof(FinetuneBatchTaskSettings), typeof(FinetuneBatchTaskSettingsPage))]
    
    class FinetuneBatchTask : AbstractFileContentProcessingAutomaticTask
    {
        private FinetuneBatchTaskSettings settings;
        private FiskmoOptions fiskmoOptions;
        private Dictionary<Language, List<ITranslationProvider>> tms;
        internal Dictionary<Language,List<Tuple<string, string>>> ProjectTranslations;

        public Dictionary<Language, List<string>> ProjectNewSegments { get; private set; }

        protected override void OnInitializeTask()
        {
            this.settings = GetSetting<FinetuneBatchTaskSettings>();
            this.fiskmoOptions = new FiskmoOptions(new Uri(this.settings.ProviderOptions));
            this.tms = this.InstantiateProjectTms();
            this.ProjectTranslations = new Dictionary<Language, List<Tuple<string, string>>>();
            this.ProjectNewSegments = new Dictionary<Language, List<string>>();
            base.OnInitializeTask();
        }

        protected override void ConfigureConverter(ProjectFile projectFile, IMultiFileConverter multiFileConverter)
        {
            //Get instances of the translation memories included in the project.
            
            var language = projectFile.GetLanguageDirection().TargetLanguage;
            FileReader _task = new FileReader(tms[language],settings);
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
        private Dictionary<Language,List<ITranslationProvider>> InstantiateProjectTms()
        {
            var tms = new Dictionary<Language, List<ITranslationProvider>>();
            foreach (var lang in this.Project.GetProjectInfo().TargetLanguages)
            {
                var tpConfig = this.Project.GetTranslationProviderConfiguration(lang);
                List<ITranslationProvider> tps = new List<ITranslationProvider>();
                foreach (var entry in tpConfig.Entries)
                {
                    var uri = entry.MainTranslationProvider.Uri;
                    //only instantiate file TMs, users can use server tms by creating project TMs if neceessary
                    //handling server TM creds is too involved.
                    if (uri.Scheme.Contains("sdltm.file"))
                    {
                        var factory = TranslationProviderManager.GetTranslationProviderFactory(uri);
                        var tp = factory.CreateTranslationProvider(uri, entry.MainTranslationProvider.State, null);
                        if (tp.TranslationMethod == TranslationMethod.TranslationMemory)
                        {
                            tps.Add(tp);
                        }
                    }
                }
                tms.Add(lang, tps);
            }
            
            return tms;
        }

        public override void TaskComplete()
        {
            var projectInfo = this.Project.GetProjectInfo();
            var projectGuid = projectInfo.Id;
            var sourceCode = projectInfo.SourceLanguage.CultureInfo.TwoLetterISOLanguageName;

            

            if (settings.AddFiskmoProvider)
            {
                //Add Fiskmö MT provider to the project
                var tpConfig = this.Project.GetTranslationProviderConfiguration();

                var fiskmoRef = new TranslationProviderReference(fiskmoOptions.Uri);
                tpConfig.Entries.Add(
                    new TranslationProviderCascadeEntry(fiskmoRef, false, true, false));

                this.Project.UpdateTranslationProviderConfiguration(tpConfig);
            }

            if (settings.Finetune)
            {
                foreach (var targetLang in projectInfo.TargetLanguages)
                {
                    var targetCode = targetLang.CultureInfo.TwoLetterISOLanguageName;
                    //Remove duplicates
                    var uniqueProjectTranslations = this.ProjectTranslations[targetLang].Distinct().ToList();
                    var uniqueNewSegments = this.ProjectNewSegments[targetLang].Distinct().ToList();
                    //Send the tuning set to MT service
                    FiskmöMTServiceHelper.Customize(
                        this.fiskmoOptions.mtServiceAddress,
                        this.fiskmoOptions.mtServicePort,
                        uniqueProjectTranslations,
                        uniqueNewSegments,
                        sourceCode, 
                        targetCode, 
                        this.fiskmoOptions.modelTag,
                        this.settings.IncludePlaceholderTags);
                }
            }

            
            //Send the new segments to MT engine for pretranslation
            foreach (var targetLang in projectInfo.TargetLanguages)
            {
                var targetCode = targetLang.CultureInfo.TwoLetterISOLanguageName;
                var uniqueNewSegments = this.ProjectNewSegments[targetLang].Distinct().ToList();
                //Send the new segments to MT service
                FiskmöMTServiceHelper.PreTranslateBatch(fiskmoOptions.mtServiceAddress, fiskmoOptions.mtServicePort, uniqueNewSegments, sourceCode, targetCode, projectGuid.ToString());
            }

        }
    }
}
