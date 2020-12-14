using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Sdl.LanguagePlatform.Core;
using Sdl.LanguagePlatform.TranslationMemoryApi;
using Sdl.ProjectAutomation.AutomaticTasks;
using Sdl.ProjectAutomation.Core;
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
        
        /*private static FiskmoOptions GetActiveOpusTpOptions(Document doc)
        {
            //Make sure that the project has an active Fiskmö translation provider included in it.
            var projectTpConfig = project.GetTranslationProviderConfiguration();

            //Check if language-specific tp config overrides the main config
            var targetLanguageTpConfig = project.GetTranslationProviderConfiguration(langDir.TargetLanguage);

            TranslationProviderCascadeEntry activeFiskmoTp;
            if (targetLanguageTpConfig.OverrideParent)
            {
                activeFiskmoTp = targetLanguageTpConfig.Entries.SingleOrDefault(
                x =>
                    x.MainTranslationProvider.Enabled &&
                    x.MainTranslationProvider.Uri.OriginalString.Contains(FiskmoTranslationProviderScheme)
                );
            }
            else
            {
                activeFiskmoTp = projectTpConfig.Entries.SingleOrDefault(
                x =>
                    x.MainTranslationProvider.Enabled &&
                    x.MainTranslationProvider.Uri.OriginalString.Contains(FiskmoTranslationProviderScheme)
                );
            }
        }*/

        //Whenever doc changes, start translating the segments and caching translations
        private static void DocChanged(object sender, DocumentEventArgs e)
        {
            if (e.Document == null)
            {
                return;
            }

            var project = e.Document.Project;
            var projectInfo = project.GetProjectInfo();

            LanguageDirection langDir;

            //Check whether document contains files
            if (e.Document.Files.Any())
            {
                langDir = e.Document.Files.First().GetLanguageDirection();
            }
            else
            {
                return;
            }

            //Make sure that the project has an active Fiskmö translation provider included in it.
            var projectTpConfig = project.GetTranslationProviderConfiguration();
            
            //Check if language-specific tp config overrides the main config
            var targetLanguageTpConfig = project.GetTranslationProviderConfiguration(langDir.TargetLanguage);

            TranslationProviderCascadeEntry activeFiskmoTp;
            if (targetLanguageTpConfig.OverrideParent)
            {
                activeFiskmoTp = targetLanguageTpConfig.Entries.SingleOrDefault(
                x =>
                    x.MainTranslationProvider.Enabled &&
                    x.MainTranslationProvider.Uri.OriginalString.Contains(FiskmoTranslationProviderScheme)
                );
            }
            else
            {
                activeFiskmoTp = projectTpConfig.Entries.SingleOrDefault(
                x =>
                    x.MainTranslationProvider.Enabled &&
                    x.MainTranslationProvider.Uri.OriginalString.Contains(FiskmoTranslationProviderScheme)
                );
            }
            
            if (activeFiskmoTp != null)
            {
                 
                var activeFiskmoOptions = new FiskmoOptions(activeFiskmoTp.MainTranslationProvider.Uri);

                if (activeFiskmoOptions.pregenerateMt)
                {
                    //The previous solution for pregeneration was to start translating the
                    //whole document as soon as the doc changes. This has a problem:
                    //if you have a massive document, just opening the document will cause a massive
                    //load on the translation service.
                    //So instead this was changed to add a segment changed handler which order only a certain
                    //amount on new translations for the next n segments whenever segment changes.
                    //Previous solution is provided below, commented out.

                    e.Document.ActiveSegmentChanged += (x,y) => segmentChanged(activeFiskmoOptions, langDir, x, y);

                    //Add a collection for tracking which documents have been preprocessed for each lang pair
                    /*if (!FiskmoProvider.processedDocuments.ContainsKey(langDir))
                    {
                        FiskmoProvider.processedDocuments.Add(langDir, new ConcurrentBag<Document>());
                    }

                    //If a document has been processed already, don't process it again
                    if (!FiskmoProvider.processedDocuments[langDir].Contains(e.Document))
                    {
                        System.Threading.Tasks.Task t = System.Threading.Tasks.Task.Run(() => TranslateDocumentSegments(e.Document, langDir, activeFiskmoOptions));
                    }*/
                }
            }
            else
            {
                return;
            }
            
        }

        private static void segmentChanged(FiskmoOptions options, LanguageDirection langDir, object sender, EventArgs e)
        {
            var doc = (Document)sender;
            var visitor = new FiskmoMarkupDataVisitor();
            
            //TODO: time this to see if it's a bottleneck during translation.
            //If this is too slow, it might be best to go with a doc changed handler that would collect all the source texts
            //once as soon as the doc is changed and then you could use that collection to run the 
            //next segment checks.
            //TESTED: doesn't seem slow at all, probably the translation part later that causes delay.
            var nextSegmentPairs = doc.SegmentPairs.SkipWhile(x =>
                !(x.Properties.Id == doc.ActiveSegmentPair.Properties.Id &&
                x.GetParagraphUnitProperties().ParagraphUnitId == doc.ActiveSegmentPair.GetParagraphUnitProperties().ParagraphUnitId)).Take(10);

            foreach (var segmentPair in nextSegmentPairs)
            {
                if (segmentPair.Properties.ConfirmationLevel == Sdl.Core.Globalization.ConfirmationLevel.Unspecified)
                {
                    visitor.Reset();
                    segmentPair.Source.AcceptVisitor(visitor);
                    var sourceText = visitor.PlainText;
                    
                    var sourceCode = langDir.SourceLanguage.CultureInfo.TwoLetterISOLanguageName;
                    var targetCode = langDir.TargetLanguage.CultureInfo.TwoLetterISOLanguageName;
                    var langpair = $"{sourceCode}-{targetCode}";

                    //The preorder method is async, so it doesn't block execution
                    FiskmöMTServiceHelper.PreOrder(options, sourceText, sourceCode, targetCode, options.modelTag);
                }
            }
        }

        //THIS IS DEPRECATED, REPLACED WITH SEGMENT CHANGE HANDLER EVENT
        //This function starts translating all segments in the document once the document is opened,
        //so that the translator won't have to wait for the translation to finish when opening a segment.
        //Note that Studio contains a feature called LookAhead which attempts to do a similar thing, but
        //LookAhead appears to be buggy with TMs etc., so it's better to rely on a custom caching system.
        private static void TranslateDocumentSegments(Document doc, LanguageDirection langPair, FiskmoOptions options)
        {
            var visitor = new FiskmoMarkupDataVisitor();
            EditorController editorController = SdlTradosStudio.Application.GetController<EditorController>();
            foreach (var segmentPair in doc.SegmentPairs)
            {
                if (segmentPair.Properties.ConfirmationLevel == Sdl.Core.Globalization.ConfirmationLevel.Unspecified)
                {
                    visitor.Reset();
                    segmentPair.Source.AcceptVisitor(visitor);
                    var sourceText = visitor.PlainText;

                    var sourceCode = langPair.SourceLanguage.CultureInfo.TwoLetterISOLanguageName;
                    var targetCode = langPair.TargetLanguage.CultureInfo.TwoLetterISOLanguageName;
                    var langpair = $"{sourceCode}-{targetCode}";

                    //This will generate the translation and cache it for later use
                    FiskmöMTServiceHelper.Translate(options, sourceText, sourceCode, targetCode, options.modelTag);
                }
            }

            //processedDocuments[langPair].Add(doc);
        }

        public FiskmoProvider(FiskmoOptions options)
        {
            Options = options;
            
            //If we create a provider with the pregenerate on, add a doc change handler to start preordering
            //MT when doc is changed
            if (options.pregenerateMt)
            {
                EditorController editorController = SdlTradosStudio.Application.GetController<EditorController>();
                //This should ensure the handler is only attached once, by always removing a possible previously
                //added handler before adding the new one
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

