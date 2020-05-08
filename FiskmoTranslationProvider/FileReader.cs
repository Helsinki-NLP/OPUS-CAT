

using Sdl.Core.Globalization;
using Sdl.FileTypeSupport.Framework.BilingualApi;
using Sdl.LanguagePlatform.TranslationMemoryApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FiskmoTranslationProvider
{
    public class FileReader : AbstractBilingualContentProcessor
    {
        internal List<Tuple<string,string>> FileTranslations;
        internal List<string> FileNewSegments;
        internal List<Tuple<string, string>> TmFuzzies;
        private FinetuneBatchTaskSettings settings;
        private string localFilePath;
        Dictionary<Language, List<ITranslationProvider>> tms;

        public FileReader(Dictionary<Language, List<ITranslationProvider>> tms, FinetuneBatchTaskSettings settings)
        {
            this.settings = settings;
            this.FileTranslations = new List<Tuple<string, string>>();
            this.FileNewSegments = new List<string>();
            this.TmFuzzies = new List<Tuple<string, string>>();
            this.tms = tms;
        }

        private string ExtractSegmentText(ISegment segment)
        {
            if (segment.ToString().Contains("\n"))
            {
                return "";
            }
            StringBuilder segmentText = new StringBuilder();
            foreach (var item in segment.AllSubItems)
            {
                if (item is IText)
                {
                    segmentText.Append(item);
                }
                else if (settings.IncludePlaceholderTags && item is IPlaceholderTag)
                {
                    //segmentText.Append(((IPlaceholderTag)item).TagProperties.DisplayText);
                    segmentText.Append("PLACEHOLDER");
                }
            }

            return segmentText.ToString();
        }

        public override void ProcessParagraphUnit(IParagraphUnit paragraphUnit)
        {
            // Check if this paragraph actually contains segments 
            // If not, it is just a structure tag content, which is not processed
            if (paragraphUnit.IsStructure)
            {
                return;
            }

            foreach (ISegmentPair segmentPair in paragraphUnit.SegmentPairs)
            {
                if (segmentPair.Properties.ConfirmationLevel == Sdl.Core.Globalization.ConfirmationLevel.Translated ||
                        segmentPair.Properties.ConfirmationLevel == Sdl.Core.Globalization.ConfirmationLevel.ApprovedTranslation)
                {
                    FileTranslations.Add(new Tuple<string, string>(
                        this.ExtractSegmentText(segmentPair.Source),
                        this.ExtractSegmentText(segmentPair.Target)));
                }
                else
                {
                    //If segment does not have translation, add it to new strings and look for fuzzies
                    FileNewSegments.Add(this.ExtractSegmentText(segmentPair.Source));

                    foreach (var tm in this.tms)
                    {
                        
                    }
                }
            }
        }
    }
}