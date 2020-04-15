using Sdl.ProjectAutomation.AutomaticTasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sdl.FileTypeSupport.Framework.IntegrationApi;
using Sdl.ProjectAutomation.Core;

namespace FiskmoTranslationProvider
{
    [AutomaticTask("My_Custom_Batch_Task_ID",
                   "My_Custom_Batch_Task_Name",
                   "My_Custom_Batch_Task_Description",
                   //[TODO] You can change the file type according to your needs
                   GeneratedFileType = AutomaticTaskFileType.None)]
    //[TODO] You can change the file type according to your needs
    [AutomaticTaskSupportedFileType(AutomaticTaskFileType.BilingualTarget)]
    [RequiresSettings(typeof(CustomizeBatchTaskSettings), typeof(CustomizeBatchTaskSettingsPage))]
    class CustomizeBatchTask : AbstractFileContentProcessingAutomaticTask
    {
        internal List<Tuple<string, string>> ProjectTranslations;

        protected override void OnInitializeTask()
        {
            this.ProjectTranslations = new List<Tuple<string, string>>();
            base.OnInitializeTask();
        }

        protected override void ConfigureConverter(ProjectFile projectFile, IMultiFileConverter multiFileConverter)
        {
            FileReader _task = new FileReader();
            multiFileConverter.AddBilingualProcessor(_task);
            multiFileConverter.Parse();
            this.ProjectTranslations.AddRange(_task.FileTranslations);
        }

        public override void TaskComplete()
        {
            var projectInfo = this.Project.GetProjectInfo();
            var sourceCode = projectInfo.SourceLanguage.CultureInfo.TwoLetterISOLanguageName;
            var mtServicePort = FiskmoTpSettings.Default.MtServicePort;
            foreach (var targetLang in projectInfo.TargetLanguages)
            {
                var targetCode = targetLang.CultureInfo.TwoLetterISOLanguageName;
                //Send the tuning set to MT service
                FiskmöMTServiceHelper.Customize(mtServicePort, this.ProjectTranslations, sourceCode, targetCode);
            }
        }
    }
}
