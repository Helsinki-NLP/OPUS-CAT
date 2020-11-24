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
        Dictionary<int, List<TranslationUnit>> tmMatches;
        public Dictionary<int, List<TranslationUnit>> TmMatches { get => tmMatches; set => tmMatches = value; }
        HashSet<string> allTmMatches;

        internal void Extract()
        {
            this.ExtractFullMatches();
            this.ExtractFuzzies();
        }

        private int transUnitCount;
        IEnumerable<string> sourceSegments;
        IEnumerable<int> fuzzyBands;
        
        public FinetuneTransUnitExtractor(
            IEnumerable<ITranslationProviderLanguageDirection> tms,
            IEnumerable<string> sourceSegments,
            IEnumerable<int> fuzzyBands)
        {
            this.tms = tms;
            this.sourceSegments = sourceSegments;
            this.fuzzyBands = fuzzyBands;
            this.tmMatches = new Dictionary<int, List<TranslationUnit>>();
            this.allTmMatches = new HashSet<string>();
        }

        private void ExtractFullMatches()
        {
            this.tmMatches[100] = new List<TranslationUnit>();
            foreach (var sourceSegment in this.sourceSegments)
            {
                this.RunTmSearch(sourceSegment, 100, 10, SearchMode.NormalSearch);
            }
        }

        private void ExtractFuzzies()
        {
            
            foreach (var fuzzyBand in this.fuzzyBands)
            {
                this.tmMatches[fuzzyBand] = new List<TranslationUnit>();
                foreach (var sourceSegment in this.sourceSegments)
                {
                    this.RunTmSearch(sourceSegment, fuzzyBand, 50, SearchMode.NormalSearch);
                }
            }
        }

        private void ExtractConcordanceMatches()
        {

        }

        private void ExtractTmSegments()
        {

        }

        private void RunTmSearch(string sourceText, int minScore, int maxResults, SearchMode searchMode)
        {
            SearchSettings searchSettings = new SearchSettings();
            searchSettings.Mode = searchMode;

            searchSettings.MinScore = minScore;
            searchSettings.MaxResults = maxResults;

            this.TmMatches[minScore] = new List<TranslationUnit>();

            foreach (var tm in this.tms)
            {
                var results = tm.SearchText(searchSettings, sourceText);
                
                foreach (var res in results)
                {
                    //This match might have been saved as part of previous fuzzy band
                    if (!this.allTmMatches.Contains(res.MemoryTranslationUnit.SourceSegment.ToPlain()))
                    {
                        this.TmMatches[minScore].Add(res.MemoryTranslationUnit);
                        this.allTmMatches.Add(res.MemoryTranslationUnit.SourceSegment.ToPlain());
                    }
                }
                
                this.transUnitCount += results.Count;
            }
        }
    }
}
