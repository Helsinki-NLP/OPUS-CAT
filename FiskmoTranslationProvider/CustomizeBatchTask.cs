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
                   GeneratedFileType = AutomaticTaskFileType.BilingualTarget)]
    //[TODO] You can change the file type according to your needs
    [AutomaticTaskSupportedFileType(AutomaticTaskFileType.BilingualTarget)]
    [RequiresSettings(typeof(CustomizeBatchTaskSettings), typeof(CustomizeBatchTaskSettingsPage))]
    class MyCustomBatchTask : AbstractFileContentProcessingAutomaticTask
    {
        protected override void OnInitializeTask()
        {
            base.OnInitializeTask();
        }
        protected override void ConfigureConverter(ProjectFile projectFile, IMultiFileConverter multiFileConverter)
        {
            //In here you should add your custom bilingual processor to the file converter
            throw new NotImplementedException();
        }
    }
}
