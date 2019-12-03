using System;
using Sdl.LanguagePlatform.Core;
using Sdl.LanguagePlatform.TranslationMemoryApi;

namespace FiskmoTranslationProvider
{
    public class FiskmoProvider : ITranslationProvider
    {
        ///<summary>
        /// This string needs to be a unique value.
        /// This is the string that precedes the plug-in URI.
        ///</summary>    
        public static readonly string ListTranslationProviderScheme = "fiskmoprovider";

        #region "ListTranslationOptions"
        public FiskmoOptions Options
        {
            get;
            set;
        }

        public FiskmoProvider(FiskmoOptions options)
        {
            Options = options;

        }
        #endregion

        #region "ITranslationProvider Members"

        public ITranslationProviderLanguageDirection GetLanguageDirection(LanguagePair languageDirection)
        {
            return new FiskmoProviderLanguageDirection(this, languageDirection);
        }

        public bool IsReadOnly
        {
            get { return true; }
        }

        public void LoadState(string translationProviderState)
        {
        }

        public string Name
        {
            get { return PluginResources.Plugin_NiceName; }
        }

        public void RefreshStatusInfo()
        {
            
        }

        public string SerializeState()
        {
            // Save settings
            return null;
        }


        public ProviderStatusInfo StatusInfo
        {
            get { return new ProviderStatusInfo(true, PluginResources.Plugin_NiceName); }
        }

        #region "SupportsConcordanceSearch"
        public bool SupportsConcordanceSearch
        {
            get { return false; }
        }
        #endregion

        public bool SupportsDocumentSearches
        {
            get { return false; }
        }

        public bool SupportsFilters
        {
            get { return false; }
        }

        #region "SupportsFuzzySearch"
        public bool SupportsFuzzySearch
        {
            get { return false; }
        }
        #endregion

        
        /// <summary>
        /// Determines the language direction of the delimited list file by
        /// reading the first line. Based upon this information it is determined
        /// whether the plug-in supports the language pair that was selected by
        /// the user.
        /// </summary>
        #region "SupportsLanguageDirection"
        public bool SupportsLanguageDirection(LanguagePair languageDirection)
        {
            var sourceCode = languageDirection.SourceCulture.TwoLetterISOLanguageName;
            var targetCode = languageDirection.TargetCulture.TwoLetterISOLanguageName;
            /*if ((sourceCode == "sv" && targetCode == "fi") || (sourceCode == "fi" && targetCode == "sv"))
            {
                return true;
            }
            else
            {
                return false;
            }*/

            var modelManager = new ModelManager();
            return modelManager.IsLanguagePairSupported(sourceCode, targetCode);
        }
        #endregion


        #region "SupportsMultipleResults"
        public bool SupportsMultipleResults
        {
            get { return false; }
        }
        #endregion

        #region "SupportsPenalties"
        public bool SupportsPenalties
        {
            get { return false; }
        }
        #endregion

        public bool SupportsPlaceables
        {
            get { return false; }
        }

        public bool SupportsScoring
        {
            get { return false; }
        }

        #region "SupportsSearchForTranslationUnits"
        public bool SupportsSearchForTranslationUnits
        {
            get { return false; }
        }
        #endregion

        #region "SupportsSourceTargetConcordanceSearch"
        public bool SupportsSourceConcordanceSearch
        {
            get { return false; }
        }

        public bool SupportsTargetConcordanceSearch
        {
            get { return false; }
        }
        #endregion

        public bool SupportsStructureContext
        {
            get { return false; }
        }

        #region "SupportsTaggedInput"
        public bool SupportsTaggedInput
        {
            get { return false; }
        }
        #endregion


        public bool SupportsTranslation
        {
            get { return true; }
        }

        #region "SupportsUpdate"
        public bool SupportsUpdate
        {
            get { return false; }
        }
        #endregion

        public bool SupportsWordCounts
        {
            get { return false; }
        }

        public TranslationMethod TranslationMethod
        {
            get { return FiskmoOptions.ProviderTranslationMethod; }
        }

        #region "Uri"
        public Uri Uri
        {
            get { return Options.Uri; }
        }
        #endregion

        #endregion
    }
}

