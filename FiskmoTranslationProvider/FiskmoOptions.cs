using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows.Input;
using Sdl.LanguagePlatform.TranslationMemoryApi;

namespace FiskmoTranslationProvider
{
    /// <summary>
    /// This class is used to hold the provider plug-in settings. 
    /// All settings are automatically stored in a URI.
    /// </summary>
    public class FiskmoOptions : INotifyPropertyChanged, IDataErrorInfo
    {

        #region "TranslationMethod"
        public static readonly TranslationMethod ProviderTranslationMethod = TranslationMethod.MachineTranslation;
        #endregion

        #region "TranslationProviderUriBuilder"
        TranslationProviderUriBuilder _uriBuilder;

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public string this[string columnName]
        {
            get
            {
                return Validate(columnName);
            }
        }

        public string Error
        {
            get { return "...."; }
        }

        private void ServicePortBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]");
            e.Handled = regex.IsMatch(e.Text);
        }

        private string Validate(string propertyName)
        {
            // Return error message if there is error on else return empty or null string
            string validationMessage = string.Empty;
            switch (propertyName)
            {
                case "mtServicePort":
                    if (this.mtServicePort != null && this.mtServicePort != "")
                    {
                        var portNumber = Int32.Parse(this.mtServicePort);
                        if (portNumber < 1024 || portNumber > 65535)
                        {
                            validationMessage = "Error";
                        }
                    }
                    else
                    {
                        validationMessage = "Error";
                    }

                    break;
            }

            return validationMessage;
        }

        public FiskmoOptions()
        {
            _uriBuilder = new TranslationProviderUriBuilder(FiskmoProvider.FiskmoTranslationProviderScheme);
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
                    //Add default to URI
                    SetStringParameter("mtServicePort", FiskmoTpSettings.Default.MtServicePort);
                    return FiskmoTpSettings.Default.MtServicePort;
                }
                else
                {
                    return parameter;
                }
            }
            set { SetStringParameter("mtServicePort", value); NotifyPropertyChanged(); }
        }

        public string mtServiceAddress
        {
            get
            {
                var parameter = GetStringParameter("mtServiceAddress");
                if (parameter == "" || parameter == null)
                {
                    //Add default to URI
                    SetStringParameter("mtServiceAddress", FiskmoTpSettings.Default.MtServiceAddress);
                    return FiskmoTpSettings.Default.MtServiceAddress;
                }
                else
                {
                    return parameter;
                }
            }
            set { SetStringParameter("mtServiceAddress", value); NotifyPropertyChanged(); }
        }

        public string modelTag
        {
            get
            {
                var parameter = GetStringParameter("modelTag");
                if (parameter == "" || parameter == null)
                {
                    return "";
                }
                else
                {
                    return parameter;
                }
            }
            set { SetStringParameter("modelTag", value); NotifyPropertyChanged(); }
        }

        public Boolean pregenerateMt
        {
            get { return GetBooleanParameter("pregenerateMt"); }
            set { SetBooleanParameter("pregenerateMt", value); NotifyPropertyChanged(); }
        }


        public Boolean showMtAsOrigin
        {
            get { return GetBooleanParameter("showMtAsOrigin"); }
            set { SetBooleanParameter("showMtAsOrigin", value); NotifyPropertyChanged(); }
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
                //Default the parameter to make it visible in the URI
                SetBooleanParameter(p, false);
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
