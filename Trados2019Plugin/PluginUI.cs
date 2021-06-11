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


        [Action("InsertOpusCatTranslation", Name ="Insert OPUS-CAT translation", Icon = "opus.bmp", Description = "Insert the currently displayed OPUS-CAT translation")]
        public class MyMainIconAction : AbstractViewControllerAction<EditorController>
        {
            protected override void Execute()
            {
                
                //get the active segment pair from the current active document in the editor
                var activeSegmentPair = Controller.ActiveDocument.ActiveSegmentPair;
                if (activeSegmentPair == null) return;

                //Create an instance of the document item factory that is needed to create segment elements
                IDocumentItemFactory documentItemFactory = DefaultDocumentItemFactory.CreateInstance();
                
                ITextProperties firstTextProp = documentItemFactory
                    .PropertiesFactory
                    .CreateTextProperties(OpusCatProviderLanguageDirection.CurrentTranslation.ToPlain());
                IText firstText = documentItemFactory.CreateText(firstTextProp);
                
                activeSegmentPair.Target.Add(firstText);
                Controller.ActiveDocument.UpdateSegmentPair(activeSegmentPair);
            }
        }
    }
}
