using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sdl.FileTypeSupport.Framework.BilingualApi;
using Sdl.LanguagePlatform.Core;
using Sdl.LanguagePlatform.TranslationMemoryApi;
using Sdl.ProjectAutomation.AutomaticTasks;
using Sdl.TranslationStudioAutomation.IntegrationApi;

namespace FiskmoTranslationProvider
{
    public class FiskmoProvider : ITranslationProvider
    {
        ///<summary>
        /// This string needs to be a unique value.
        /// This is the string that precedes the plug-in URI.
        ///</summary>    
        public static readonly string FiskmoTranslationProviderScheme = "fiskmoprovider";

        #region "ListTranslationOptions"
        public FiskmoOptions Options
        {
            get;
            set;
        }

        private static Dictionary<LanguageDirection, ConcurrentBag<Document>> processedDocuments = new Dictionary<LanguageDirection, ConcurrentBag<Document>>();

        //Whenever doc changes, start translating the segments and caching translations
        private static void DocChanged(object sender, DocumentEventArgs e)
        {
            if (e.Document == null)
            {
                return;
            }

            var project = e.Document.Project;
            var projectInfo = project.GetProjectInfo();

            //Make sure that the project has an active Fiskmö translation provider included in it.
            var projectTpConfig = project.GetTranslationProviderConfiguration();
            var tpEntries = projectTpConfig.Entries;
            var activeFiskmoTp = tpEntries.SingleOrDefault(
                x =>
                    x.MainTranslationProvider.Enabled &&
                    x.MainTranslationProvider.Uri.OriginalString.Contains(FiskmoTranslationProviderScheme)
                );


            
            
            if (e.Document.Files.Count() > 0 && activeFiskmoTp != null)
            {
                var activeFiskmoOptions = new FiskmoOptions(activeFiskmoTp.MainTranslationProvider.Uri);
                var langPair = e.Document.ActiveFile.GetLanguageDirection();
                if (!FiskmoProvider.processedDocuments.ContainsKey(langPair))
                {
                    FiskmoProvider.processedDocuments.Add(langPair, new ConcurrentBag<Document>());
                }

                if (activeFiskmoTp != null &&
                    activeFiskmoOptions.pregenerateMt &&
                    !FiskmoProvider.processedDocuments[langPair].Contains(e.Document))
                {
                    Task t = Task.Run(() => TranslateDocumentSegments(e.Document, langPair, activeFiskmoOptions));
                }
            }
            else
            {
                return;
            }
            
        }

        //This function starts translating all segments in the document once the document is opened,
        //so that the translator won't have to wait for the translation to finish when opening a segment.
        //Note that Studio contains a feature called LookAhead which attempts to do a similar thing, but
        //this feature appears to be buggy with TMs etc., so it's better to rely on a custom caching system.
        private static void TranslateDocumentSegments(Document doc, LanguageDirection langPair, FiskmoOptions options)
        {
            EditorController editorController = SdlTradosStudio.Application.GetController<EditorController>();
            foreach (var segmentPair in doc.SegmentPairs)
            {
                if (segmentPair.Properties.ConfirmationLevel == Sdl.Core.Globalization.ConfirmationLevel.Unspecified)
                {
                    var allTextItems = segmentPair.Source.AllSubItems.Where(x => x is IText);
                    var sourceText = String.Join(" ", allTextItems);

                    var sourceCode = langPair.SourceLanguage.CultureInfo.TwoLetterISOLanguageName;
                    var targetCode = langPair.TargetLanguage.CultureInfo.TwoLetterISOLanguageName;
                    var langpair = $"{sourceCode}-{targetCode}";

                    //This will generate the translation and cache it for later use
                    FiskmöMTServiceHelper.Translate(options, sourceText, sourceCode, targetCode, options.modelTag);

                }
            }

            processedDocuments[langPair].Add(doc);
        }

        public FiskmoProvider(FiskmoOptions options)
        {
            Options = options;

            if (options.pregenerateMt)
            {
                EditorController editorController = SdlTradosStudio.Application.GetController<EditorController>();
                editorController.ActiveDocumentChanged -= FiskmoProvider.DocChanged;
                editorController.ActiveDocumentChanged += FiskmoProvider.DocChanged;
            }

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
        /// It seems that this method is called many times (possibly for each segment) by Trados.
        /// Consequently nothing that requires long waits should be added here.
        /// 
        /// As Fiskmo theoretically supports any language direction, set this to always return true.
        /// </summary>
        #region "SupportsLanguageDirection"
        public bool SupportsLanguageDirection(LanguagePair languageDirection)
        {
            return true;
            /*var sourceCode = languageDirection.SourceCulture.TwoLetterISOLanguageName;
            var targetCode = languageDirection.TargetCulture.TwoLetterISOLanguageName;

            var supportedLanguagePairs = FiskmöMTServiceHelper.ListSupportedLanguages(this.Options);
            return supportedLanguagePairs.Contains($"{sourceCode}-{targetCode}");*/
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
            get { return true; }
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

