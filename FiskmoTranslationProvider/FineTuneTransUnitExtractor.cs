using Sdl.LanguagePlatform.TranslationMemory;
using Sdl.LanguagePlatform.TranslationMemoryApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FiskmoTranslationProvider
{
    class FinetuneTransUnitExtractor
    {
        /// <summary>
        /// This class extracts translation units based on the new segments collected from project files.
        /// The extraction proceeds through several phases:
        /// 1. Full match extraction: full matches are extracted from TM
        /// 2. Fuzzy match extraction: fuzzy matches are extracted for different fuzzy bands.
        /// 3. Concordance match extraction: concordance matches are extracted for part of the source sentence.
        /// 4. Straight extraction from TM, newest first.
        /// 
        /// The goal is to extract a given number of most relevant translation units. Step 4 is a backoff,
        /// it means padding the finetuning set with material that doesn't directly relate to the job at hand
        /// (but which still comes from the relevant domain, i.e. the same TM).
        /// </summary>


        IEnumerable<ITranslationProviderLanguageDirection> tms;
        Dictionary<int, IEnumerable<TranslationUnit>> tmMatches;
        private int transUnitCount;
        IEnumerable<string> sourceSegments;

        private void ExtractFullMatches()
        {
            foreach (var sourceSegment in this.sourceSegments)
            {
                this.RunTmSearch(100, 10, sourceSegment);
            }
            
        }

        private void ExtractFuzzies()
        {

        }

        private void ExtractConcordanceMatches()
        {

        }

        private void ExtractTmSegments()
        {

        }

        private void RunTmSearch(int minScore, int maxResults, string sourceText)
        {
            SearchSettings searchSettings = new SearchSettings();
            searchSettings.Mode = SearchMode.NormalSearch;

            searchSettings.MinScore = 95;
            searchSettings.MaxResults = 5;

            foreach (var tm in this.tms)
            {
                var results = tm.SearchText(searchSettings, sourceText);
                this.tmMatches[minScore] = results.Select(x => x.MemoryTranslationUnit);
                this.transUnitCount += results.Count;
            }
        }
    }
}
