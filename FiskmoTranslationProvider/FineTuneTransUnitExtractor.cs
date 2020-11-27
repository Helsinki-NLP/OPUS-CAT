using Newtonsoft.Json;
using Sdl.LanguagePlatform.TranslationMemory;
using Sdl.LanguagePlatform.TranslationMemoryApi;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
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

        static FinetuneTransUnitExtractor()
        {
            var assembly = Assembly.GetExecutingAssembly();
            using (var isoTable = new StreamReader(assembly.GetManifestResourceStream("TransunitExtractionTester.stopwords-iso.json")))
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

        HashSet<string> allTmMatches;

        internal void Extract()
        {
            //this.ExtractFullMatches();
            //this.ExtractFuzzies();
            //this.ExtractConcordanceMatches();
            this.ExtractFillerUnits();
        }

        private int transUnitCount;
        IEnumerable<string> sourceSegments;
        IEnumerable<int> fuzzyBands;
        private List<TranslationUnit> concordanceMatches;

        public FinetuneTransUnitExtractor(
            IEnumerable<ITranslationMemoryLanguageDirection> tms,
            IEnumerable<string> sourceSegments,
            IEnumerable<int> fuzzyBands)
        {
            this.tms = tms;
            this.sourceLanguage = tms.First().SourceLanguage.TwoLetterISOLanguageName;
            this.sourceSegments = sourceSegments;
            this.fuzzyBands = fuzzyBands;
            this.tmMatches = new Dictionary<int, List<TranslationUnit>>();
            this.allTmMatches = new HashSet<string>();
        }

        private void ExtractFillerUnits()
        {
            //Extract segments from the end of the TM, with the assumption that
            //the last segments are the newest. This might not be the case, though,
            //it seems the TMs are ordered by insertion date, which might be different
            //from creation date (e.g. if tm has been processed in some way).
            foreach (var tm in this.tms)
            {
                RegularIterator iterator = new RegularIterator();
                iterator.Forward = false;
                iterator.PositionFrom = tm.GetTranslationUnitCount();
                var test = tm.GetTranslationUnits(ref iterator);
            }
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

        private void ExtractConcordanceMatches(int maxWindow=2)
        {
            this.ConcordanceMatches = new List<TranslationUnit>();
            foreach (var sourceSegment in this.sourceSegments)
            {
                //partition the source segment for concordance search
                for (var window = 1; window <= maxWindow; window++)
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

                        if (this.allTmMatches.Contains(windowString))
                        {
                            continue;
                        }
                        else
                        {
                            this.allTmMatches.Add(windowString);
                        }

                        var results = this.RunTmSearch(windowString, 100, 10, SearchMode.ConcordanceSearch);
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
                    //This match might have been saved as part of previous fuzzy band
                    if (!this.allTmMatches.Contains(res.MemoryTranslationUnit.SourceSegment.ToPlain()))
                    {
                        newResults.Add(res.MemoryTranslationUnit);
                        this.allTmMatches.Add(res.MemoryTranslationUnit.SourceSegment.ToPlain());
                    }
                }
                
                this.transUnitCount += results.Count;
            }

            return newResults;
        }
    }
}
