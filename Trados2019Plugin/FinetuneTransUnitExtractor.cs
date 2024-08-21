using Newtonsoft.Json;
using OpusCatMtEngine;
using Sdl.LanguagePlatform.TranslationMemory;
using Sdl.LanguagePlatform.TranslationMemoryApi;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OpusCatTranslationProvider
{

    static class ShuffleExtension
    {
        public static void Swap<T>(this IList<T> list, int i, int j)
        {
            var temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }

        public static void Shuffle<T>(this IList<T> list, Random rnd)
        {
            for (var i = list.Count; i > 0; i--)
                list.Swap(0, rnd.Next(0, i));
        }
    }

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

        static FinetuneTransUnitExtractor()
        {
            var assembly = Assembly.GetExecutingAssembly();
            using (var isoTable = new StreamReader(assembly.GetManifestResourceStream("OpusCatTranslationProvider.stopwords-iso.json")))
            {
                FinetuneTransUnitExtractor.stopWords = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(isoTable.ReadToEnd());
            }
        }

        private static Dictionary<string, List<string>> stopWords;


        IEnumerable<ITranslationMemoryLanguageDirection> tms;
        private string sourceLanguage;
        Dictionary<int, List<TranslationUnit>> tmMatches;
        public Dictionary<int, List<TranslationUnit>> TmMatches { get => tmMatches; set => tmMatches = value; }
        public List<TranslationUnit> ConcordanceMatches { get => concordanceMatches; set => concordanceMatches = value; }
        public List<TranslationUnit> FillerUnits { get => fillerUnits; set => fillerUnits = value; }
        public List<ParallelSentence> AllExtractedTranslations
        {
            get
            {
                var allUnits =
                    this.TmMatches.SelectMany(x => x.Value).Concat(
                        this.ConcordanceMatches).Concat(
                        this.FillerUnits);
                var allTranslations = new List<ParallelSentence>();

                var sourceVisitor = new OpusCatProviderElementVisitor();
                var targetVisitor = new OpusCatProviderElementVisitor();
                foreach (var unit in allUnits)
                {
                    sourceVisitor.Reset();
                    targetVisitor.Reset();
                    foreach (var element in unit.SourceSegment.Elements)
                    {
                        element.AcceptSegmentElementVisitor(sourceVisitor);
                    }
                    foreach (var element in unit.TargetSegment.Elements)
                    {
                        element.AcceptSegmentElementVisitor(targetVisitor);
                    }

                    allTranslations.Add(new ParallelSentence(sourceVisitor.PlainText,targetVisitor.PlainText));
                }

                return allTranslations;
            }
        }

        public int MaxConcordanceMatchesPerSearch { get; private set; }
        public int MaxFuzzyMatchesPerSearch { get; private set; }
        
        HashSet<string> allTmMatchSourceTexts;
        private int unitsNeeded;

        internal void Extract(
            bool includeFuzzies=true,
            bool includeConcordanceUnits=true,
            bool includeFillerUnits = true)
        {
            //Note that all methods will first extract matches from TMs sequentially,
            //i.e. the latter TMs might not be used at all, if the first TM has enough material.
            
            this.ExtractFullMatches();

            if (this.unitsNeeded > 0 && includeFuzzies)
            {
                this.ExtractFuzzies();
            }

            if (this.unitsNeeded > 0 && includeConcordanceUnits)
            {
                this.ExtractConcordanceMatches();
            }

            if (this.unitsNeeded > 0 && includeFillerUnits)
            {
                this.ExtractFillerUnits();
            }
        }

        List<string> sourceSegments;
        IEnumerable<int> fuzzyBands;
        private List<TranslationUnit> concordanceMatches;
        private List<TranslationUnit> fillerUnits;
        private int maxConcordanceWindow;

        public FinetuneTransUnitExtractor(
            IEnumerable<ITranslationMemoryLanguageDirection> tms,
            IEnumerable<string> sourceSegments,
            IEnumerable<int> fuzzyBands,
            int unitsNeeded,
            int maxConcordanceMatchesPerSearch,
            int maxFuzzyMatchesPerSearch,
            int maxConcordanceWindow)
        {
            this.tms = tms;
#if (TRADOS22)
            this.sourceLanguage = new CultureInfo(tms.First().SourceLanguage.Name).TwoLetterISOLanguageName;
#else
            this.sourceLanguage = tms.First().SourceLanguage.TwoLetterISOLanguageName;
#endif
            //Shuffle the source segment to prevent focusing on the initial part of the job (in case
            //units needed value is reached before whole source has been processed).
            this.sourceSegments = sourceSegments.ToList();
            this.sourceSegments.Shuffle(new Random());
            this.fuzzyBands = fuzzyBands;

            this.tmMatches = new Dictionary<int, List<TranslationUnit>>();
            this.ConcordanceMatches = new List<TranslationUnit>();
            this.FillerUnits = new List<TranslationUnit>();

            this.allTmMatchSourceTexts = new HashSet<string>();
            this.unitsNeeded = unitsNeeded;
            this.MaxConcordanceMatchesPerSearch = maxConcordanceMatchesPerSearch;
            this.MaxFuzzyMatchesPerSearch = maxFuzzyMatchesPerSearch;
            this.maxConcordanceWindow = maxConcordanceWindow;
        }

        private void ExtractFillerUnits()
        {

            //Extract segments from the end of the TM, with the assumption that
            //the last segments are the newest. This might not be the case, though,
            //it seems the TMs are ordered by insertion date, which might be different
            //from creation date (e.g. if tm has been processed in some way).

            foreach (var tm in this.tms)
            {
                RegularIterator iterator = new RegularIterator
                {
                    Forward = false,
                    PositionFrom = tm.GetTranslationUnitCount()
                };

                TranslationUnit[] fillerBatch = tm.GetTranslationUnits(ref iterator);
                while (fillerBatch != null && fillerBatch.Length > 0)
                {
                    fillerBatch = tm.GetTranslationUnits(ref iterator);
                    FillerUnits.AddRange(fillerBatch);
                    if (FillerUnits.Count > this.unitsNeeded)
                    {
                        this.FillerUnits.RemoveRange(this.unitsNeeded, this.FillerUnits.Count - this.unitsNeeded);
                        return;
                    }
                }
            }

            return;
        }

        private void ExtractFullMatches()
        {
            this.tmMatches[100] = new List<TranslationUnit>();
            foreach (var sourceSegment in this.sourceSegments)
            {
                this.tmMatches[100].AddRange(this.RunTmSearch(sourceSegment, 100, 10, SearchMode.NormalSearch));
            }
        }

        private void ExtractFuzzies()
        {
            
            foreach (var fuzzyBand in this.fuzzyBands)
            {
                this.tmMatches[fuzzyBand] = new List<TranslationUnit>();
                foreach (var sourceSegment in this.sourceSegments)
                {
                    this.tmMatches[fuzzyBand].AddRange(this.RunTmSearch(sourceSegment, fuzzyBand, 50, SearchMode.NormalSearch));
                }
            }
        }

        private void ExtractConcordanceMatches()
        {
            foreach (var sourceSegment in this.sourceSegments)
            {
                //partition the source segment for concordance search
                for (var window = 1; window <= this.maxConcordanceWindow; window++)
                {
                    var sourceSplit = Regex.Split(sourceSegment, @"[ \p{P}]").Where(x => !String.IsNullOrEmpty(x)).ToList();

                    if (sourceSplit.Count == 0)
                    {
                        continue;
                    }

                    for (var startIndex = 0; startIndex <= sourceSplit.Count-window; startIndex++)
                    {
                        //Do stopword filtering
                        var windowTokens = sourceSplit.GetRange(startIndex, window);
                        
                        if (FinetuneTransUnitExtractor.stopWords.ContainsKey(this.sourceLanguage))
                        {
                            //if all tokens are function words, skip the loop cycle
                            if (windowTokens.All(x => FinetuneTransUnitExtractor.stopWords[this.sourceLanguage].Contains(x)))
                            {
                                continue;
                            }
                        }

                        var windowString = String.Join(" ", windowTokens);

                        if (this.allTmMatchSourceTexts.Contains(windowString))
                        {
                            continue;
                        }
                        else
                        {
                            this.allTmMatchSourceTexts.Add(windowString);
                        }

                        var results = this.RunTmSearch(windowString, 100, this.MaxConcordanceMatchesPerSearch, SearchMode.ConcordanceSearch);
                        var resultText = results.Select(x => x.TargetSegment.ToPlain());
                        this.ConcordanceMatches.AddRange(results);
                    }                    
                }
            }
        }

        
        private IEnumerable<TranslationUnit> RunTmSearch(string sourceText, int minScore, int maxResults, SearchMode searchMode)
        {
            SearchSettings searchSettings = new SearchSettings();
            searchSettings.Mode = searchMode;

            searchSettings.MinScore = minScore;
            searchSettings.MaxResults = maxResults;

            //Only records matches that haven't been stored yet
            var newResults = new List<TranslationUnit>();

            foreach (var tm in this.tms)
            {
                var results = tm.SearchText(searchSettings, sourceText);
                
                foreach (var res in results)
                {
                    if (this.unitsNeeded <= 0)
                    {
                        return newResults;
                    }

                    //This match might have been saved as part of previous fuzzy band.
                    //If there are multiple transunits with same source, this should pick the newest
                    //(assumed to be most correct).
                    if (!this.allTmMatchSourceTexts.Contains(res.MemoryTranslationUnit.SourceSegment.ToPlain()))
                    {
                        newResults.Add(res.MemoryTranslationUnit);
                        this.allTmMatchSourceTexts.Add(res.MemoryTranslationUnit.SourceSegment.ToPlain());
                        this.unitsNeeded--;
                    }
                }
            }

            return newResults;
        }
    }
}
