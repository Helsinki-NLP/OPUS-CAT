using System;
using System.Drawing;
using MemoQ.MTInterfaces;

namespace OpusMTPlugin
{
    /// <summary>
    /// Dummy MT engine for a particular language combination.
    /// </summary>
    /// <remarks>
    /// Implementation checklist:
    ///     - The MTException class is used to wrap the original exceptions occurred during the translation.
    ///     - All allocated resources are disposed correctly in the session.
    /// </remarks>
    public class OpusMTEngine : EngineBase
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
        private readonly OpusMTOptions options;

        public OpusMTEngine(string srcLangCode, string trgLangCode, OpusMTOptions options)
        {
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
            return new OpusMTSession(srcLangCode, trgLangCode, options);
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
            get { return null; }
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
            return new OpusMTSession(srcLangCode, trgLangCode, options);
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
