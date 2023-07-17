using System;
using System.Collections.Generic;
using Sdl.LanguagePlatform.Core;
using Sdl.LanguagePlatform.TranslationMemory;
using Sdl.LanguagePlatform.TranslationMemoryApi;
using System.Collections.Specialized;
using System.Diagnostics;
using Sdl.TranslationStudioAutomation.IntegrationApi;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Linq;
using Sdl.FileTypeSupport.Framework.BilingualApi;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Runtime.InteropServices;
using System.Globalization;
using Sdl.Core.Globalization;

namespace OpusCatTranslationProvider
{
    public class OpusCatProviderLanguageDirection : ITranslationProviderLanguageDirection
    {
        #region "PrivateMembers"
        private OpusCatProvider _provider;
        private LanguagePair _languageDirection;
        private OpusCatOptions _options;
        private OpusCatProviderElementVisitor _visitor;
      
        private string langpair;
        
        #endregion

 

        /// <summary>
        /// Instantiates the variables and fills the list file content into
        /// a Dictionary collection object.
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="languages"></param>
        #region "ListTranslationProviderLanguageDirection"
        public OpusCatProviderLanguageDirection(OpusCatProvider provider, LanguagePair languages)
        {
            #region "Instantiate"
            
            _provider = provider;
            _languageDirection = languages;
            _options = _provider.Options;

            _visitor = new OpusCatProviderElementVisitor();

#if (TRADOS22)
            var sourceCode = new CultureInfo(this._languageDirection.SourceCulture.Name).TwoLetterISOLanguageName;
            var targetCode = new CultureInfo(this._languageDirection.TargetCulture.Name).TwoLetterISOLanguageName;
#else
            var sourceCode = this._languageDirection.SourceCulture.TwoLetterISOLanguageName;
            var targetCode = this._languageDirection.TargetCulture.TwoLetterISOLanguageName;
#endif

            this.langpair = $"{sourceCode}-{targetCode}";

#endregion
        }



        public System.Globalization.CultureInfo SourceLanguage
        {
            get { return _languageDirection.SourceCulture; }
        }

        public System.Globalization.CultureInfo TargetLanguage
        {
            get { return _languageDirection.TargetCulture; }
        }

        public ITranslationProvider TranslationProvider
        {
            get { return _provider; }
        }


        /// <summary>
        /// Performs the actual search
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="segment"></param>
        /// <returns></returns>
#region "SearchSegment"
        public SearchResults SearchSegment(SearchSettings settings, Segment segment)
        {
           
            _visitor.Reset();
            foreach (var element in segment.Elements)
            {
                element.AcceptSegmentElementVisitor(_visitor);
            }
            

#region "SearchResultsObject"
            SearchResults results = new SearchResults();
            results.SourceSegment = segment.Duplicate();

#endregion
            string sourceText = _visitor.PlainText;

#if (TRADOS22)
            var sourceCode = new CultureInfo(this._languageDirection.SourceCulture.Name).TwoLetterISOLanguageName;
            var targetCode = new CultureInfo(this._languageDirection.TargetCulture.Name).TwoLetterISOLanguageName;
#else
            var sourceCode = this._languageDirection.SourceCulture.TwoLetterISOLanguageName;
            var targetCode = this._languageDirection.TargetCulture.TwoLetterISOLanguageName;
#endif

            var langpair = $"{sourceCode}-{targetCode}";

            List<SearchResult> systemResults = this.GenerateSystemResult(sourceText, settings.Mode,segment,sourceCode,targetCode);
            foreach (var res in systemResults)
            {
                results.Add(res);
            }
 
            return results;
#endregion
        }

        private List<SearchResult> GenerateSystemResult(
            string sourceText, 
            SearchMode mode, 
            Segment segment, string sourceCode, string targetCode)
        {
            List<SearchResult> systemResults = new List<SearchResult>();

            Segment translationSegment = new Segment(_languageDirection.TargetCulture);
            if (this._options.opusCatSource == OpusCatOptions.OpusCatSource.OpusCatMtEngine)
            {
                var translation = OpusCatProvider.OpusCatMtEngineConnection.Translate(
                    this._options.mtServiceAddress, 
                    this._options.mtServicePort, 
                    sourceText, 
                    sourceCode, 
                    targetCode, 
                    this._options.modelTag);

                if (this._options.restoreTags &&
                    (_visitor.Placeholders.Any() || 
                    _visitor.TagStarts.Any() || 
                    _visitor.TagEnds.Any()))
                {
                    var tagRestorer = new TagRestorer(segment, translation, _visitor, translationSegment);
                    tagRestorer.ProcessTags();
                }
                else
                {
                    translationSegment.Add(translation.Translation);
                }

                //Fix potential tag problems
                translationSegment.FillUnmatchedStartAndEndTags();
                if (!translationSegment.IsValid())
                {
                    translationSegment.Clear();
                    translationSegment.Add(translation.Translation);
                }
            }
            else if (this._options.opusCatSource == OpusCatOptions.OpusCatSource.Elg)
            {
                var taglessSourceText = Regex.Replace(sourceText, " (PLACEHOLDER|TAGPAIRSTART|TAGPAIREND) ", "");
                var translatedSentence = OpusCatProvider.ElgConnection.Translate(taglessSourceText,
                    sourceCode, 
                    targetCode);
                translationSegment.Add(translatedSentence);
            }
            else
            {
                translationSegment = null;
            }

            if (translationSegment == null)
                return systemResults;

            
            // Look up the currently selected segment in the collection (normal segment lookup).
            if (mode == SearchMode.FullSearch || mode == SearchMode.NormalSearch)
            {
                
                systemResults.Add(CreateSearchResult(segment, translationSegment, segment.HasTags,"opus-cat"));
            }
            return systemResults;
        }
#endregion



        /// <summary>
        /// Creates the translation unit as it is later shown in the Translation Results
        /// window of SDL Trados Studio.
        /// </summary>
        /// <param name="searchSegment"></param>
        /// <param name="translation"></param>
        /// <param name="sourceSegment"></param>
        /// <param name="formattingPenalty"></param>
        /// <returns></returns>
#region "CreateSearchResult"
        private SearchResult CreateSearchResult(Segment searchSegment, Segment translation,
            bool formattingPenalty,string mtSystem)
        {
            
            TranslationUnit tu = new TranslationUnit();
            
            tu.SourceSegment = searchSegment;
            tu.TargetSegment = translation;

            //There might be some problematic tags after the tag stack treatment, this should
            //handle those.
            //TODO: Ideally the tag issues would be resolved already in the NMT decoder, i.e. only
            //insertion of matched tags would be allowed.
            var error = tu.Validate(Segment.ValidationMode.ReportAllErrors);
            
            //This is a fallback for tag errors
            if (!error.HasFlag(ErrorCode.OK))
            {
                tu.TargetSegment.DeleteTags();
            }

            OpusCatProviderLanguageDirection.CurrentTranslation = tu.TargetSegment;

            tu.ResourceId = new PersistentObjectToken(tu.GetHashCode(), Guid.Empty);
            tu.FieldValues.Add(new SingleStringFieldValue("mtSystem", mtSystem));
            
            if (this._options.showMtAsOrigin)
            {
#if (TRADOS21 || TRADOS22)
                tu.Origin = TranslationUnitOrigin.Nmt;
#else
                tu.Origin = TranslationUnitOrigin.MachineTranslation;
#endif
            }
            else
            {
                tu.Origin = TranslationUnitOrigin.Unknown;
            }
            
            SearchResult searchResult = new SearchResult(tu);
            searchResult.ScoringResult = new ScoringResult();

            return searchResult;
        }
#endregion


        public bool CanReverseLanguageDirection
        {
            get { return false; }
        }

        public static Segment CurrentTranslation { get; private set; }

#if (TRADOS22)
        CultureCode ITranslationProviderLanguageDirection.SourceLanguage => new CultureCode(this.SourceLanguage);
        CultureCode ITranslationProviderLanguageDirection.TargetLanguage => new CultureCode(this.TargetLanguage);
#endif

        public SearchResults[] SearchSegments(SearchSettings settings, Segment[] segments)
        {
            SearchResults[] results = new SearchResults[segments.Length];
            for (int p = 0; p < segments.Length; ++p)
            {
                results[p] = SearchSegment(settings, segments[p]);
            }
            return results;
        }

        public SearchResults[] SearchSegmentsMasked(SearchSettings settings, Segment[] segments, bool[] mask)
        {
            if (segments == null)
            {
                throw new ArgumentNullException("segments in SearchSegmentsMasked");
            }
            if (mask == null || mask.Length != segments.Length)
            {
                throw new ArgumentException("mask in SearchSegmentsMasked");
            }

            SearchResults[] results = new SearchResults[segments.Length];
            for (int p = 0; p < segments.Length; ++p)
            {
                if (mask[p])
                {
                    results[p] = SearchSegment(settings, segments[p]);
                }
                else
                {
                    results[p] = null;
                }
            }

            return results;
        }

        public SearchResults SearchText(SearchSettings settings, string segment)
        {
            Segment s = new Segment(_languageDirection.SourceCulture);
            s.Add(segment);
            return SearchSegment(settings, s);
        }

        public SearchResults SearchTranslationUnit(SearchSettings settings, TranslationUnit translationUnit)
        {
            return SearchSegment(settings, translationUnit.SourceSegment);
        }

        public SearchResults[] SearchTranslationUnits(SearchSettings settings, TranslationUnit[] translationUnits)
        {
            SearchResults[] results = new SearchResults[translationUnits.Length];
            for (int p = 0; p < translationUnits.Length; ++p)
            {
                results[p] = SearchSegment(settings, translationUnits[p].SourceSegment);
            }
            return results;
        }

        public SearchResults[] SearchTranslationUnitsMasked(SearchSettings settings, TranslationUnit[] translationUnits, bool[] mask)
        {
            List<SearchResults> results = new List<SearchResults>();

            int i = 0;
            foreach (var tu in translationUnits)
            {
                if (mask == null || mask[i])
                {
                    var result = SearchTranslationUnit(settings, tu);
                    results.Add(result);
                }
                else
                {
                    results.Add(null);
                }
                i++;
            }

            return results.ToArray();
        }


#region "NotForThisImplementation"
        /// <summary>
        /// Not required for this implementation.
        /// </summary>
        /// <param name="translationUnits"></param>
        /// <param name="settings"></param>
        /// <param name="mask"></param>
        /// <returns></returns>
        public ImportResult[] AddTranslationUnitsMasked(TranslationUnit[] translationUnits, ImportSettings settings, bool[] mask)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Not required for this implementation.
        /// </summary>
        /// <param name="translationUnit"></param>
        /// <returns></returns>
        public ImportResult UpdateTranslationUnit(TranslationUnit translationUnit)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Not required for this implementation.
        /// </summary>
        /// <param name="translationUnits"></param>
        /// <returns></returns>
        public ImportResult[] UpdateTranslationUnits(TranslationUnit[] translationUnits)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Not required for this implementation.
        /// </summary>
        /// <param name="translationUnits"></param>
        /// <param name="previousTranslationHashes"></param>
        /// <param name="settings"></param>
        /// <param name="mask"></param>
        /// <returns></returns>
        public ImportResult[] AddOrUpdateTranslationUnitsMasked(TranslationUnit[] translationUnits, int[] previousTranslationHashes, ImportSettings settings, bool[] mask)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Not required for this implementation.
        /// </summary>
        /// <param name="translationUnit"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        public ImportResult AddTranslationUnit(TranslationUnit translationUnit, ImportSettings settings)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Not required for this implementation.
        /// </summary>
        /// <param name="translationUnits"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        public ImportResult[] AddTranslationUnits(TranslationUnit[] translationUnits, ImportSettings settings)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Not required for this implementation.
        /// </summary>
        /// <param name="translationUnits"></param>
        /// <param name="previousTranslationHashes"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        public ImportResult[] AddOrUpdateTranslationUnits(TranslationUnit[] translationUnits, int[] previousTranslationHashes, ImportSettings settings)
        {
            throw new NotImplementedException();
        }
#endregion
    }
}
