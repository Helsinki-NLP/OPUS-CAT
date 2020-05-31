using System;
using System.Collections.Generic;
using Sdl.LanguagePlatform.Core;
using Sdl.LanguagePlatform.TranslationMemory;
using Sdl.LanguagePlatform.TranslationMemoryApi;
using System.Collections.Specialized;
using System.Diagnostics;
using Sdl.TranslationStudioAutomation.IntegrationApi;
using Sdl.TranslationStudioAutomation.IntegrationApi.Extensions;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Linq;
using Sdl.FileTypeSupport.Framework.BilingualApi;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Runtime.InteropServices;

namespace FiskmoTranslationProvider
{
    public class FiskmoProviderLanguageDirection : ITranslationProviderLanguageDirection
    {
        #region "PrivateMembers"
        private FiskmoProvider _provider;
        private LanguagePair _languageDirection;
        private FiskmoOptions _options;
        private FiskmoProviderElementVisitor _visitor;
      
        private string langpair;
        
        #endregion

 

        /// <summary>
        /// Instantiates the variables and fills the list file content into
        /// a Dictionary collection object.
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="languages"></param>
        #region "ListTranslationProviderLanguageDirection"
        public FiskmoProviderLanguageDirection(FiskmoProvider provider, LanguagePair languages)
        {
            #region "Instantiate"
            // UT.LogMessageToFile("Init ListTranslationProviderLanguageDirection");
            
            _provider = provider;
            _languageDirection = languages;
            _options = _provider.Options;

            _visitor = new FiskmoProviderElementVisitor();

            var sourceCode = this._languageDirection.SourceCulture.TwoLetterISOLanguageName;
            var targetCode = this._languageDirection.TargetCulture.TwoLetterISOLanguageName;
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
           
            var sourceCode = this._languageDirection.SourceCulture.TwoLetterISOLanguageName;
            var targetCode = this._languageDirection.TargetCulture.TwoLetterISOLanguageName;
            var langpair = $"{sourceCode}-{targetCode}";

            List<SearchResult> systemResults = this.GenerateSystemResult(sourceText, settings.Mode,segment,sourceCode,targetCode);
            foreach (var res in systemResults)
            {
                results.Add(res);
            }
 
            return results;
            #endregion
        }

        private List<SearchResult> GenerateSystemResult(string sourceText, SearchMode mode, Segment segment, string sourceCode, string targetCode)
        {
            List<SearchResult> systemResults = new List<SearchResult>();
            string translatedSentence = FiskmöMTServiceHelper.Translate(this._options, sourceText, sourceCode, targetCode,this._options.modelTag);

            if (String.IsNullOrEmpty(translatedSentence))
                return systemResults;

            // Look up the currently selected segment in the collection (normal segment lookup).
            if (mode == SearchMode.FullSearch || mode == SearchMode.NormalSearch)
            {
                Segment translation = new Segment(_languageDirection.TargetCulture);
                if (_visitor.Placeholders.Any() || _visitor.TagStarts.Any() || _visitor.TagEnds.Any())
                {

                    var split = Regex.Split(translatedSentence, @"\b(PLACEHOLDER|TAGPAIRSTART ?| ?TAGPAIREND)\b");
                
                    //Tag starts and ends must match, so need a stack to keep track of what tags
                    //have been applied
                    var tagStack = new Stack<Tag>();

                    foreach (var part in split)
                    {
                        //Remove potential spaces from after TAGPAIRSTARTS and before TAGPAIREND
                        var normalpart = part.Replace("TAGPAIRSTART ", "TAGPAIRSTART");
                        normalpart = normalpart.Replace(" TAGPAIREND", "TAGPAIREND");

                        switch (normalpart)
                        {
                            case "PLACEHOLDER":
                                if (_visitor.Placeholders.Count != 0)
                                {
                                    translation.Add(_visitor.Placeholders.Dequeue());
                                }
                                break;
                            case "TAGPAIRSTART":
                                if (_visitor.TagStarts.Count != 0)
                                {
                                    var startTag = _visitor.TagStarts.Dequeue();
                                    tagStack.Push(startTag);
                                    translation.Add(startTag);
                                }
                                break;
                            case "TAGPAIREND":
                                if (tagStack.Count != 0)
                                {
                                    var correspondingStartTag = tagStack.Pop();
                                    var endTag = _visitor.TagEnds[correspondingStartTag.TagID];
                                    translation.Add(endTag);
                                }
                                break;
                            default:
                                translation.Add(part);
                                break;
                        }
                    }

                    //Insert missing end tags
                    foreach (var excessStartTag in tagStack)
                    {
                        var nonEndedTagIndex = translation.Elements.IndexOf(excessStartTag);
                        var endTag = _visitor.TagEnds[excessStartTag.TagID];
                        translation.Elements.Insert(nonEndedTagIndex+1, endTag);
                    }
                }
                else
                {
                    translation.Add(translatedSentence);
                }

                
                systemResults.Add(CreateSearchResult(segment, translation, segment.HasTags,"fiskmö"));
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

            FiskmoProviderLanguageDirection.CurrentTranslation = tu.TargetSegment;

            tu.ResourceId = new PersistentObjectToken(tu.GetHashCode(), Guid.Empty);
            tu.FieldValues.Add(new SingleStringFieldValue("mtSystem", mtSystem));
            
            if (this._options.showMtAsOrigin)
            {
                tu.Origin = TranslationUnitOrigin.MachineTranslation;
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
