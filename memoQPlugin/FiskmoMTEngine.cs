using System;
using System.Drawing;
using System.Reflection;
using MemoQ.Addins.Common.Utils;
using MemoQ.MTInterfaces;

namespace FiskmoMTPlugin
{
    /// <summary>
    /// Dummy MT engine for a particular language combination.
    /// </summary>
    /// <remarks>
    /// Implementation checklist:
    ///     - The MTException class is used to wrap the original exceptions occurred during the translation.
    ///     - All allocated resources are disposed correctly in the session.
    /// </remarks>
    public class FiskmoMTEngine : EngineBase
    {
        /// <summary>
        /// The source language.
        /// </summary>
        private readonly string srcLangCode;

        /// <summary>
        /// The target language.
        /// </summary>
        private readonly string trgLangCode;

        /// <summary>
        /// Plugin options
        /// </summary>
        private readonly FiskmoMTOptions options;

        public FiskmoMTEngine(string srcLangCode, string trgLangCode, FiskmoMTOptions options)
        {
            var test = LanguageHelper.GetIsoCode2LetterFromIsoCode3Letter(trgLangCode);
            var lang = new Language(trgLangCode);
            
            this.srcLangCode = srcLangCode;
            this.trgLangCode = trgLangCode;
            this.options = options;
        }

        #region IEngine Members

        /// <summary>
        /// Creates a session for translating segments. Session will not be used in a multi-threaded way.
        /// </summary>
        public override ISession CreateLookupSession()
        {
            return new FiskmoMTSession(srcLangCode, trgLangCode, options);
        }

        /// <summary>
        /// Set an engine-specific custom property, e.g., subject matter area.
        /// </summary>
        public override void SetProperty(string name, string value)
        {
            // not needed
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns a small icon to be displayed under translation results when an MT hit is selected from this plugin.
        /// </summary>
        public override Image SmallIcon
        {
            get
            {
                var image = Image.FromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("FiskmöMTPlugin.fiskmö_small.png"));
                return image;
            }
        }

        /// <summary>
        /// Indicates whether the engine supports the adjustment of fuzzy TM hits through machine translation.
        /// </summary>
        public override bool SupportsFuzzyCorrection
        {
            get { return false; }
        }

        /// <summary>
        /// Creates a session for translating segments. Session will not be used in a multi-threaded way.
        /// </summary>
        public override ISessionForStoringTranslations CreateStoreTranslationSession()
        {
            return new FiskmoMTSession(srcLangCode, trgLangCode, options);
        }

        #endregion

        #region IDisposable Members

        public override void Dispose()
        {
            // dispose your resources if needed
        }

        #endregion
    }
}
