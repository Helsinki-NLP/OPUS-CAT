

using Sdl.Core.Globalization;
using Sdl.FileTypeSupport.Framework.BilingualApi;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FiskmoTranslationProvider
{
    public class FileReader : AbstractBilingualContentProcessor
    {
        internal List<Tuple<string,string>> FileTranslations;
        private object _settings;
        private string localFilePath;

        public FileReader()
        {
            this.FileTranslations = new List<Tuple<string, string>>();
        }

        public override void ProcessParagraphUnit(IParagraphUnit paragraphUnit)
        {
            // Check if this paragraph actually contains segments 
            // If not, it is just a structure tag content, which is not processed
            if (paragraphUnit.IsStructure)
            {
                return;
            }

            // If the paragraph contains segment pairs, we loop through them,
            // determine their confirmation status, and depending on the status
            // output the text content to a TXT file
            foreach (ISegmentPair segmentPair in paragraphUnit.SegmentPairs)
            {
                if (segmentPair.Properties.ConfirmationLevel == Sdl.Core.Globalization.ConfirmationLevel.Translated ||
                        segmentPair.Properties.ConfirmationLevel == Sdl.Core.Globalization.ConfirmationLevel.ApprovedTranslation)
                {
                    var allSourceTextItems = segmentPair.Source.AllSubItems.Where(x => x is IText);
                    var sourceText = String.Join(" ", allSourceTextItems);

                    var allTargetTextItems = segmentPair.Target.AllSubItems.Where(x => x is IText);
                    var targetText = String.Join(" ", allTargetTextItems);

                    FileTranslations.Add(new Tuple<string, string>(sourceText, targetText));
                }
            }
        }
    }
}