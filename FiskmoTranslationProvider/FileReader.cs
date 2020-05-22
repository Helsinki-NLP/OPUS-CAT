

using Sdl.Core.Globalization;
using Sdl.FileTypeSupport.Framework.BilingualApi;
using Sdl.LanguagePlatform.TranslationMemory;
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
        internal List<TranslationUnit> TmFuzzies;
        private FinetuneBatchTaskSettings settings;
        private FiskmoOptions options;
        IEnumerable<ITranslationProviderLanguageDirection> tmLanguageDirections;

        public FileReader(IEnumerable<ITranslationProviderLanguageDirection> tms, FinetuneBatchTaskSettings settings)
        {
            this.settings = settings;
            this.options = new FiskmoOptions(new Uri(settings.ProviderOptions));
            this.FileTranslations = new List<Tuple<string, string>>();
            this.FileNewSegments = new List<string>();
            this.TmFuzzies = new List<TranslationUnit>();
            this.tmLanguageDirections = tms;
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

                    SearchSettings searchSettings = new SearchSettings();
                    searchSettings.Mode = SearchMode.NormalSearch;
                    searchSettings.MinScore = settings.FuzzyMinPercentage;
                    searchSettings.MaxResults = settings.FuzzyMaxResults;

                    foreach (var tmLangDir in this.tmLanguageDirections)
                    {
                        var results = tmLangDir.SearchText(searchSettings, segmentPair.Source.ToString());
                        this.TmFuzzies.AddRange(results.Select(x => x.MemoryTranslationUnit));
                    }

                }
            }
        }
    }
}