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
using Sdl.ProjectAutomation.FileBased;
using Sdl.TranslationStudioAutomation.IntegrationApi;

namespace OpusCatTranslationProvider
{
    public class OpusCatProvider : ITranslationProvider
    {
        ///<summary>
        /// This string needs to be a unique value.
        /// This is the string that precedes the plug-in URI.
        ///</summary>    
        public static readonly string OpusCatTranslationProviderScheme = "opuscatprovider";
        private static ConcurrentDictionary<Document,List<EventHandler>> activeSegmentHandlers = new ConcurrentDictionary<Document, List<EventHandler>>();
        
        public OpusCatOptions Options
        {
            get;
            set;
        }

#if TRADOS21
        private static IStudioDocument activeDocument;
#else
        private static Document activeDocument;

#endif

        internal static void UpdateSegmentHandler()
        {
            EditorController editorController = SdlTradosStudio.Application.GetController<EditorController>();
            var activeDoc = editorController.ActiveDocument;
            if (activeDoc != null)
            {
                OpusCatProvider.UpdateSegmentHandler(activeDoc);
            }
        }

        private static IEnumerable<OpusCatOptions> GetProjectOpusCatOptions(FileBasedProject project, LanguageDirection langDir)
        {
            //Make sure that the project has an active OPUS-CAT translation provider included in it.
            var projectTpConfig = project.GetTranslationProviderConfiguration();

            //Check if language-specific tp config overrides the main config
            var targetLanguageTpConfig = project.GetTranslationProviderConfiguration(langDir.TargetLanguage);

            IEnumerable<TranslationProviderCascadeEntry> activeOpusCatTps;
            if (targetLanguageTpConfig.OverrideParent)
            {
                activeOpusCatTps = targetLanguageTpConfig.Entries.Where(
                x =>
                    x.MainTranslationProvider.Enabled &&
                    x.MainTranslationProvider.Uri.OriginalString.Contains(OpusCatTranslationProviderScheme)
                );
            }
            else
            {
                activeOpusCatTps = projectTpConfig.Entries.Where(
                x =>
                    x.MainTranslationProvider.Enabled &&
                    x.MainTranslationProvider.Uri.OriginalString.Contains(OpusCatTranslationProviderScheme)
                );
            }

            if (activeOpusCatTps.Any())
            {
                var activeOpusCatOptions = activeOpusCatTps.Select(x => new OpusCatOptions(x.MainTranslationProvider.Uri));
                return activeOpusCatOptions;
            }
            else
            {
                return null;
            }
        }

        private static void ClearSegmentHandlers()
        {
            foreach (var docHandlerListPair in OpusCatProvider.activeSegmentHandlers)
            {
                foreach (var handler in docHandlerListPair.Value)
                {
                    docHandlerListPair.Key.ActiveSegmentChanged -= handler;
                }
            }
        }

#if TRADOS21
        private static void UpdateSegmentHandler(IStudioDocument doc)
#else
        private static void UpdateSegmentHandler(Document doc)
#endif
        {
            //This method may be fired through docChanged event or through settings change.

            OpusCatProvider.ClearSegmentHandlers();

            OpusCatProvider.activeDocument = doc;

            var project = doc.Project;
            var projectInfo = project.GetProjectInfo();

            LanguageDirection langDir;

            //Check whether document contains files
            if (doc.Files.Any())
            {
                //only files of same language can be merged, so taking the langdir of first file is enough
                langDir = doc.Files.First().GetLanguageDirection();
            }
            else
            {
                return;
            }

            var activeOpusCatOptions = OpusCatProvider.GetProjectOpusCatOptions(project,langDir);

            if (activeOpusCatOptions != null)
            {
                
                if (activeOpusCatOptions.Any(x => x.pregenerateMt))
                {
                    //The previous solution for pregeneration was to start translating the
                    //whole document as soon as the doc changes. This has a problem:
                    //if you have a massive document, just opening the document will cause a massive
                    //load on the translation service.
                    //So instead this was changed to add a segment changed handler which order only a certain
                    //amount on new translations for the next n segments whenever segment changes.
                    //Previous solution is provided below, commented out.

                    //Assign the handler to field to make it possible to remove it later
                    if (!OpusCatProvider.activeSegmentHandlers.ContainsKey(doc as Document))
                    {
                        OpusCatProvider.activeSegmentHandlers[doc as Document] = new List<EventHandler>();
                    }

                    var handler = new EventHandler((x, y) => segmentChanged(langDir, x, y));
                    OpusCatProvider.activeSegmentHandlers[doc as Document].Add(handler);

                    doc.ActiveSegmentChanged += handler;
                    
                }
            }
            else
            {
                return;
            }
        }

        //Whenever doc changes, start translating the segments and caching translations
        private static void DocChanged(object sender, DocumentEventArgs e)
        {
            if (e.Document == null)
            {
                return;
            }
            
            OpusCatProvider.UpdateSegmentHandler(e.Document);
        }

        private static void segmentChanged(LanguageDirection langDir, object sender, EventArgs e)
        {
            var doc = (Document)sender;

            //There are some "segments" the Trados editor view which are not proper segments, like
            //the start of document tag
            if (doc.ActiveSegmentPair == null)
            {
                return;
            }
            var visitor = new OpusCatMarkupDataVisitor();

            var activeOpusCatOptions = OpusCatProvider.GetProjectOpusCatOptions(doc.Project, langDir);

            IEnumerable<OpusCatOptions> activeOpusCatOptionsWithPregenerate;
            if (activeOpusCatOptions == null)
            {
                activeOpusCatOptionsWithPregenerate = null;
            }
            else
            {
                activeOpusCatOptionsWithPregenerate = activeOpusCatOptions.Where(x => x.pregenerateMt);
            }
            //If there is no active OPUS CAT provider, unsubscribe this handler (there's probably no event in Trados
            //API for removing a translation provider from a project, so this is the only way to unsubscribe
            //after translation provider has been removed.
            if (activeOpusCatOptionsWithPregenerate == null || !activeOpusCatOptionsWithPregenerate.Any())
            {
                OpusCatProvider.ClearSegmentHandlers();
                return;
            }

            var sourceSegmentTexts = new List<string>();

            var nextSegmentPairs = doc.SegmentPairs.SkipWhile(x =>
                !(x.Properties.Id == doc.ActiveSegmentPair.Properties.Id &&
                x.GetParagraphUnitProperties().ParagraphUnitId == doc.ActiveSegmentPair.GetParagraphUnitProperties().ParagraphUnitId));

            var segmentsNeeded = activeOpusCatOptionsWithPregenerate.Max(x => x.pregenerateSegmentCount);
            foreach (var segmentPair in nextSegmentPairs)
            {
                if (sourceSegmentTexts.Count == segmentsNeeded)
                {
                    break;
                }
                
                //Also preorder translations for Draft segments, since quite often there will be draft content
                //provided in segments where having MT is still desirable. This could also be an option.
                if (segmentPair.Properties.ConfirmationLevel == Sdl.Core.Globalization.ConfirmationLevel.Unspecified ||
                    segmentPair.Properties.ConfirmationLevel == Sdl.Core.Globalization.ConfirmationLevel.Draft)
                {
                    visitor.Reset();
                    segmentPair.Source.AcceptVisitor(visitor);
                    var sourceText = visitor.PlainText;
                    sourceSegmentTexts.Add(sourceText);
                }
            }

            var sourceCode = langDir.SourceLanguage.CultureInfo.TwoLetterISOLanguageName;
            var targetCode = langDir.TargetLanguage.CultureInfo.TwoLetterISOLanguageName;

            foreach (var options in activeOpusCatOptionsWithPregenerate)
            {
                //The preorder method doesn't wait for the translation, so the requests return quicker
                var sourceSegmentTextsNeeded = sourceSegmentTexts.Take(options.pregenerateSegmentCount).ToList();
                OpusCatMTServiceHelper.PreOrderBatch(options, sourceSegmentTextsNeeded, sourceCode, targetCode, options.modelTag);
            }
        }

        //THIS IS DEPRECATED, REPLACED WITH SEGMENT CHANGE HANDLER EVENT
        //This function starts translating all segments in the document once the document is opened,
        //so that the translator won't have to wait for the translation to finish when opening a segment.
        //Note that Studio contains a feature called LookAhead which attempts to do a similar thing, but
        //LookAhead appears to be buggy with TMs etc., so it's better to rely on a custom caching system.
        private static void TranslateDocumentSegments(Document doc, LanguageDirection langPair, OpusCatOptions options)
        {
            var visitor = new OpusCatMarkupDataVisitor();
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
                    OpusCatMTServiceHelper.Translate(options, sourceText, sourceCode, targetCode, options.modelTag);
                }
            }

            //processedDocuments[langPair].Add(doc);
        }

        public OpusCatProvider(OpusCatOptions options, ITranslationProviderCredentialStore credentialStore)
        {
            Options = options;
            
            //If we create a provider with the pregenerate on, add a doc change handler to start preordering
            //MT when doc is changed
            if (options.pregenerateMt && options.opusCatSource == OpusCatOptions.OpusCatSource.OpusCatMtEngine)
            {
                EditorController editorController = SdlTradosStudio.Application.GetController<EditorController>();
                //This should ensure the handler is only attached once, by always removing a possible previously
                //added handler before adding the new one
                editorController.ActiveDocumentChanged -= OpusCatProvider.DocChanged;
                editorController.ActiveDocumentChanged += OpusCatProvider.DocChanged;

                //If a document is open, check if the segment change handler should be added
                OpusCatProvider.UpdateSegmentHandler();
            }

            if (this.Options.opusCatSource == OpusCatOptions.OpusCatSource.Elg)
            {
                OpusCatProvider.ElgConnection = new ElgServiceConnection(new TradosElgCredentialWrapper(credentialStore)); 
            }

        }

        #region "ITranslationProvider Members"

        public ITranslationProviderLanguageDirection GetLanguageDirection(LanguagePair languageDirection)
        {
            return new OpusCatProviderLanguageDirection(this, languageDirection);
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
        /// As OPUS-CAT theoretically supports any language direction, set this to always return true.
        /// </summary>
        #region "SupportsLanguageDirection"
        public bool SupportsLanguageDirection(LanguagePair languageDirection)
        {
            return true;
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
            get { return OpusCatOptions.ProviderTranslationMethod; }
        }

        #region "Uri"
        public Uri Uri
        {
            get { return Options.Uri; }
        }

        internal static ElgServiceConnection ElgConnection { get; private set; }
        #endregion

        #endregion
    }
}

