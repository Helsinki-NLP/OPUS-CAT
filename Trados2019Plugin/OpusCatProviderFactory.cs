using System;
using Sdl.LanguagePlatform.TranslationMemoryApi;

namespace OpusCatTranslationProvider
{
    #region "Declaration"
    [TranslationProviderFactory(
        Id = "OpusCatProviderFactory",
        Name = "OpusCatProviderFactory",
        Description = "OPUS-CAT Machine Translation.")]
    #endregion

    public class OpusCatProviderFactory : ITranslationProviderFactory
    {
        #region ITranslationProviderFactory Members

        #region "CreateTranslationProvider"
        public ITranslationProvider CreateTranslationProvider(Uri translationProviderUri, string translationProviderState, ITranslationProviderCredentialStore credentialStore)
        {
            if (!SupportsTranslationProviderUri(translationProviderUri))
            {
                throw new Exception("Cannot handle URI.");
            }

            OpusCatProvider tp = new OpusCatProvider(new OpusCatOptions(translationProviderUri),credentialStore);

            return tp;
        }
        #endregion

        #region "SupportsTranslationProviderUri"
        public bool SupportsTranslationProviderUri(Uri translationProviderUri)
        {
            if (translationProviderUri == null)
            {
                throw new ArgumentNullException("Translation provider URI not supported.");
            }
            return String.Equals(translationProviderUri.Scheme, OpusCatProvider.OpusCatTranslationProviderScheme, StringComparison.OrdinalIgnoreCase);
            //return true;
        }
        #endregion

        #region "GetTranslationProviderInfo"
        public TranslationProviderInfo GetTranslationProviderInfo(Uri translationProviderUri, string translationProviderState)
        {
            TranslationProviderInfo info = new TranslationProviderInfo();

            #region "TranslationMethod"
            info.TranslationMethod = OpusCatOptions.ProviderTranslationMethod;
            #endregion

            

            #region "Name"
            info.Name = PluginResources.Plugin_NiceName;
            #endregion

            return info;
        }
        #endregion

        #endregion
    }
}
