//using FiskmoTranslationProvider;
using FiskmoTranslationProvider;
using Sdl.Core.Globalization;
using Sdl.Core.Settings;
using Sdl.ProjectAutomation.Core;
using Sdl.ProjectAutomation.FileBased;
using Studio.AssemblyResolver;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FinetuneTestsetExtractor
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            AssemblyResolver.Resolve();
            Program.Extract(args);
        }
        
        static void Extract(string[] args)
        {
            //Make a copy of the master TM, since the finetune testset segments will need to
            //be removed from the TM before testing
            var masterTMPath = args[0];
            var batches = Int32.Parse(args[1]);
            var batchSize = Int32.Parse(args[2]);
            var masterTMNameWithoutExtension = masterTMPath.Replace(".sdltm", "");
            var filteredTMPath = $"{masterTMNameWithoutExtension}.filtered.sdltm";
            File.Copy(masterTMPath, filteredTMPath,true);

            var testsetExtractor = new TestsetExtractor(filteredTMPath, batches, masterTMNameWithoutExtension, batchSize);
            var sourceFile = testsetExtractor.ExtractTestset();
            Program.CreateProject(filteredTMPath, sourceFile, testsetExtractor.SourceLang, testsetExtractor.TargetLang);
        }

        private static void CreateProject(string tmPath, FileInfo sourceFile,Language sourceLang, Language targetLang)
        {
            ProjectInfo info = new ProjectInfo();
            
            info.SourceLanguage = sourceLang;
            info.TargetLanguages = new Language[] { targetLang };
            info.LocalProjectFolder = Path.Combine(
                sourceFile.DirectoryName,
                Path.GetFileNameWithoutExtension(sourceFile.FullName));
            info.Name = Path.GetFileNameWithoutExtension(sourceFile.FullName);

            FileBasedProject newProject = new FileBasedProject(info);

            TranslationProviderConfiguration tmConfig = newProject.GetTranslationProviderConfiguration();

            tmConfig.Entries.Add(new TranslationProviderCascadeEntry(tmPath, false, true, true));
            newProject.UpdateTranslationProviderConfiguration(tmConfig);

            newProject.AddFiles(new string[] { sourceFile.FullName });

            AutomaticTask scanFiles = newProject.RunAutomaticTask(
                newProject.GetSourceLanguageFiles().GetIds(),
                AutomaticTaskTemplateIds.Scan);

            ISettingsBundle settings = newProject.GetSettings();
            FinetuneBatchTaskSettings pretranslateSettings = settings.GetSettingsGroup<FinetuneBatchTaskSettings>();

            pretranslateSettings.Finetune = true;
            pretranslateSettings.ExtractFuzzies = true;
            pretranslateSettings.FuzzyMaxResults = 5;
            pretranslateSettings.ExtractFillerUnits = true;
            pretranslateSettings.FuzzyMinPercentage = 60;
            pretranslateSettings.BatchTranslate = false;
            pretranslateSettings.AddFiskmoProvider = false;
            pretranslateSettings.ExtractConcordanceUnits = true;
            pretranslateSettings.MaxFinetuningSentences = 100000;
            pretranslateSettings.IncludePlaceholderTags = false;
            pretranslateSettings.IncludeTagPairs = false;
            
            newProject.UpdateSettings(settings);
            newProject.Save();
            var finetuneTaskSettingsWindow = new FiskmoTranslationProvider.FinetuneWpfControl(pretranslateSettings);
            Window window = new Window
            {
                Title = "Finetune task settings",
                Content = finetuneTaskSettingsWindow
            };
            window.ShowDialog();

            

            AutomaticTask finetuneTask = newProject.RunAutomaticTask(
                newProject.GetSourceLanguageFiles().GetIds(),
                "FiskmoBatchTask");
            
            newProject.Save();
        }
        
    }
}
