using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OpusCatMTEngine
{
    /// <summary>
    /// Interaction logic for CustomizationView.xaml
    /// </summary>
    public partial class OpusCatSettingsView : UserControl, IDataErrorInfo, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }



        public OpusCatSettingsView()
        {
            InitializeComponent();
            this.ServicePortBox = OpusCatMTEngineSettings.Default.MtServicePort;
            this.HttpServicePortBox = OpusCatMTEngineSettings.Default.HttpMtServicePort;
        }

        private void OpenCustomSettingsInEditor_Click(object sender, RoutedEventArgs e)
        {
            var customizeYml = HelperFunctions.GetLocalAppDataPath(OpusCatMTEngineSettings.Default.CustomizationBaseConfig);
            Process.Start("notepad.exe",customizeYml);
        }

        
        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            OpusCatMTEngineSettings.Default.MtServicePort = this.ServicePortBox;
            OpusCatMTEngineSettings.Default.Save();
            this.SaveButtonEnabled = false;
        }

        private void httpSaveButton_Click(object sender, RoutedEventArgs e)
        {
            OpusCatMTEngineSettings.Default.HttpMtServicePort = this.HttpServicePortBox;
            OpusCatMTEngineSettings.Default.Save();
            this.HttpSaveButtonEnabled = false;
        }

        private void ServicePortBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]");
            e.Handled = regex.IsMatch(e.Text);
        }

        public string this[string columnName]
        {
            get
            {
                return Validate(columnName);
            }
        }


        private string httpServicePortBox;
        public string HttpServicePortBox
        {
            get => httpServicePortBox;
            set
            {
                httpServicePortBox = value;
                NotifyPropertyChanged();
            }
        }

        private string servicePortBox;
        public string ServicePortBox
        {
            get => servicePortBox;
            set
            {
                servicePortBox = value;
                NotifyPropertyChanged();
            }
        }

        public bool SaveButtonEnabled
        {
            get => saveButtonEnabled;
            set
            {
                saveButtonEnabled = value;
                NotifyPropertyChanged();
            }
        }

        public bool HttpSaveButtonEnabled
        {
            get => httpSaveButtonEnabled;
            set
            {
                httpSaveButtonEnabled = value;
                NotifyPropertyChanged();
            }
        }


        private bool saveButtonEnabled;
        private bool httpSaveButtonEnabled;


        public string Error
        {
            get { return "...."; }
        }


        private string Validate(string propertyName)
        {
            // Return error message if there is error on else return empty or null string
            string validationMessage = string.Empty;
            this.SaveButtonEnabled = false;
            switch (propertyName)
            {
                case "ServicePortBox":
                    if (this.ServicePortBox != null && this.ServicePortBox != "")
                    {
                        var portNumber = Int32.Parse(this.ServicePortBox);
                        if (portNumber < 1024 || portNumber > 65535)
                        {
                            validationMessage = "Error";
                        }
                        else
                        {
                            if (this.ServicePortBox != OpusCatMTEngineSettings.Default.MtServicePort)
                            {
                                this.SaveButtonEnabled = true;
                            }
                        }
                    }
                    else
                    {
                        validationMessage = "Error";
                    }

                    break;
                case "HttpServicePortBox":
                    if (this.HttpServicePortBox != null && this.HttpServicePortBox != "")
                    {
                        var portNumber = Int32.Parse(this.HttpServicePortBox);
                        if (portNumber < 1024 || portNumber > 65535)
                        {
                            validationMessage = "Error";
                        }
                        else
                        {
                            if (this.HttpServicePortBox != OpusCatMTEngineSettings.Default.HttpMtServicePort)
                            {
                                this.HttpSaveButtonEnabled = true;
                            }
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
    }
}
