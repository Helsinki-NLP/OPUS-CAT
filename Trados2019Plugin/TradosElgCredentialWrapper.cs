using Sdl.LanguagePlatform.TranslationMemoryApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpusCatTranslationProvider
{
    /// <summary>
    /// This wraps the credential store of Trados for use with ELG connection.
    /// </summary>
    internal class TradosElgCredentialWrapper : IElgCredentials
    {
        private ITranslationProviderCredentialStore credentialStore;

        static private Uri AccessTokenUri = new Uri("elgaccesstoken:///");
        static private Uri RefreshTokenUri = new Uri("elgrefreshtoken:///");

        internal TradosElgCredentialWrapper(ITranslationProviderCredentialStore credentialStore)
        {
            this.credentialStore = credentialStore;
        }
        
                
        public string AccessToken
        {
            get
            {
                return this.credentialStore.GetCredential(TradosElgCredentialWrapper.AccessTokenUri).Credential;
            }
            set
            {
                this.credentialStore.AddCredential(TradosElgCredentialWrapper.AccessTokenUri,
                    new TranslationProviderCredential(value, false));
            }
        }

        public string RefreshToken
        {
            get
            {
                return this.credentialStore.GetCredential(TradosElgCredentialWrapper.RefreshTokenUri).Credential;
            }
            set
            {
                this.credentialStore.AddCredential(TradosElgCredentialWrapper.RefreshTokenUri,
                    new TranslationProviderCredential(value, true));
            }
        }
    }
}
