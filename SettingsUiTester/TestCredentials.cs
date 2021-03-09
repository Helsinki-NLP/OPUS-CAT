using Sdl.LanguagePlatform.TranslationMemoryApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SettingsUiTester
{
    class TestCredentials : ITranslationProviderCredentialStore
    {
        Dictionary<Uri, TranslationProviderCredential> Creds;
        public void AddCredential(Uri uri, TranslationProviderCredential credential)
        {
            this.Creds[uri] = credential;
        }

        public TestCredentials()
        {
            this.Creds = new Dictionary<Uri, TranslationProviderCredential>();
        }

        public void Clear()
        {
            this.Creds = new Dictionary<Uri, TranslationProviderCredential>() ;
        }

        public TranslationProviderCredential GetCredential(Uri uri)
        {
            if (this.Creds.Keys.Contains(uri))
            {
                return this.Creds[uri];
            }
            else
            {
                return null;
            }
        }

        public void RemoveCredential(Uri uri)
        {
            if (this.Creds.Keys.Contains(uri))
            {
               this.Creds.Remove(uri);
            }
        }
    }
}
