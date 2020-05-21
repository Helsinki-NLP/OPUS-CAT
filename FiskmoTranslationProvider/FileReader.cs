

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
        private FiskmoOptions options;
        List<ITranslationProvider> tms;

        public FileReader(List<ITranslationProvider> tms, FinetuneBatchTaskSettings settings)
        {
            this.settings = settings;
            this.options = new FiskmoOptions(new Uri(settings.ProviderOptions));
            this.FileTranslations = new List<Tuple<string, string>>();
            this.FileNewSegments = new List<string>();
            this.TmFuzzies = new List<Tuple<string, string>>();
            this.tms = tms;
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
                        FiskmoProviderElementVisitor.ExtractSegmentText(segmentPair.Source),
                        FiskmoProviderElementVisitor.ExtractSegmentText(segmentPair.Target)));
                }
                else
                {
                    //If segment does not have translation, add it to new strings and look for fuzzies
                    FileNewSegments.Add(FiskmoProviderElementVisitor.ExtractSegmentText(segmentPair.Source));

                    /*foreach (var tm in this.tms[this)
                    {
                        tm
                    }*/
                }
            }
        }
    }
}