using System;
using Sdl.LanguagePlatform.TranslationMemoryApi;

namespace FiskmoTranslationProvider
{
    /// <summary>
    /// This class is used to hold the provider plug-in settings. 
    /// All settings are automatically stored in a URI.
    /// </summary>
    public class FiskmoOptions
    {
        #region "TranslationMethod"
        public static readonly TranslationMethod ProviderTranslationMethod = TranslationMethod.MachineTranslation;
        #endregion

        #region "TranslationProviderUriBuilder"
        TranslationProviderUriBuilder _uriBuilder;
        
        public FiskmoOptions()
        {
            _uriBuilder = new TranslationProviderUriBuilder(FiskmoProvider.ListTranslationProviderScheme);
            System.Console.WriteLine(FiskmoProvider.ListTranslationProviderScheme);
        }

        public FiskmoOptions(Uri uri)
        {
            _uriBuilder = new TranslationProviderUriBuilder(uri);
        }
        #endregion

        public string mtServicePort
        {
            get 
            {
                var parameter = GetStringParameter("mtServicePort");
                if (parameter == "" || parameter == null)
                {
                    return FiskmoTpSettings.Default.MtServicePort;
                }
                else
                {
                    return parameter;
                }
            }
            set { SetStringParameter("mtServicePort", value); }
        }

        /*public Boolean useAllModels
        {
            get { return GetBooleanParameter("useAllModels"); }
            set { SetBooleanParameter("useAllModels", value); }
        }*/

        /*public string serverAddress
        {
            get { return GetStringParameter("serverAddress"); }
            set { SetStringParameter("serverAddress", value); }
        }*/

        /*public string port
        {
            get { return GetStringParameter("port"); }
            set { SetStringParameter("port", value); }
        }

        public string client
        {
            get { return GetStringParameter("client"); }
            set { SetStringParameter("client", value); }
        }*/

        public Boolean pregenerateMt
        {
            get { return GetBooleanParameter("pregenerateMt"); }
            set { SetBooleanParameter("pregenerateMt", value); }
        }
        public Boolean showMtAsOrigin
        {
            get { return GetBooleanParameter("showMtAsOrigin"); }
            set { SetBooleanParameter("showMtAsOrigin", value); }
        }


        

        

        #region "SetStringParameter"
        private void SetStringParameter(string p, string value)
        {
            _uriBuilder[p] = value;
        }
        #endregion

        #region "SetBooleanParameter"
        private void SetBooleanParameter(string p, Boolean value)
        {
            _uriBuilder[p] = value.ToString();
        }
        #endregion

        #region "GetStringParameter"
        private string GetStringParameter(string p)
        {
            string paramString = _uriBuilder[p];
            return paramString;
        }
        #endregion

        #region "GetBooleanParameter"
        private Boolean GetBooleanParameter(string p)
        {
            string paramString = _uriBuilder[p];
            Boolean result;
            var parseResult = Boolean.TryParse(paramString,out result);
            if (!parseResult)
            {
                return false;
            }
            else
            {
                return result;
            }
        }
        #endregion

        #region "Uri"
        public Uri Uri
        {            
            get
            {
                return _uriBuilder.Uri;                
            }
        }
        #endregion
    }
}
