using System;
using Sdl.LanguagePlatform.TranslationMemoryApi;

namespace FiskmoTranslationProvider
{
    #region "Declaration"
    [TranslationProviderFactory(
        Id = "FiskmoProviderFactory",
        Name = "FiskmoProviderFactory",
        Description = "Fiskmo Machine Translation.")]
    #endregion

    public class FiskmoProviderFactory : ITranslationProviderFactory
    {
        #region ITranslationProviderFactory Members

        #region "CreateTranslationProvider"
        public ITranslationProvider CreateTranslationProvider(Uri translationProviderUri, string translationProviderState, ITranslationProviderCredentialStore credentialStore)
        {
            if (!SupportsTranslationProviderUri(translationProviderUri))
            {
                throw new Exception("Cannot handle URI.");
            }

            FiskmoProvider tp = new FiskmoProvider(new FiskmoOptions(translationProviderUri));

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
            return String.Equals(translationProviderUri.Scheme, FiskmoProvider.FiskmoTranslationProviderScheme, StringComparison.OrdinalIgnoreCase);
            //return true;
        }
        #endregion

        #region "GetTranslationProviderInfo"
        public TranslationProviderInfo GetTranslationProviderInfo(Uri translationProviderUri, string translationProviderState)
        {
            TranslationProviderInfo info = new TranslationProviderInfo();

            #region "TranslationMethod"
            info.TranslationMethod = FiskmoOptions.ProviderTranslationMethod;
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
