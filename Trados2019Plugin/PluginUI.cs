using Sdl.Desktop.IntegrationApi;
using Sdl.Desktop.IntegrationApi.Extensions;
using Sdl.FileTypeSupport.Framework.Core.Utilities.BilingualApi;
using Sdl.FileTypeSupport.Framework.BilingualApi;
using Sdl.FileTypeSupport.Framework.NativeApi;
using Sdl.TranslationStudioAutomation.IntegrationApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OpusCatTranslationProvider
{
    class PluginUI
    {


        [Action("InsertTranslation", Icon = "opus.bmp", Description = "Insert the currently displayed OPUS-CAT translation")]
        [Shortcut(Keys.Alt | Keys.F8)]
        public class MyMainIconAction : AbstractAction
        {
            protected override void Execute()
            {
                EditorController editorController = SdlTradosStudio.Application.GetController<EditorController>();
                //get the active segment pair from the current active document in the editor
                var activeSegmentPair = editorController.ActiveDocument.ActiveSegmentPair;
                if (activeSegmentPair == null) return;

                //Create an instance of the document item factory that is needed to create segment elements
                IDocumentItemFactory documentItemFactory = DefaultDocumentItemFactory.CreateInstance();
                
                ITextProperties firstTextProp = documentItemFactory
                    .PropertiesFactory
                    .CreateTextProperties(OpusCatProviderLanguageDirection.CurrentTranslation.ToPlain());
                IText firstText = documentItemFactory.CreateText(firstTextProp);
                
                activeSegmentPair.Target.Add(firstText);
                editorController.ActiveDocument.UpdateSegmentPair(activeSegmentPair);
            }
        }
    }
}
