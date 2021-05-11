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
            
            this.Loaded += SettingsControl_Loaded;
        }


        private void SettingsControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= SettingsControl_Loaded;
            
            this.ServicePortBox = OpusCatMTEngineSettings.Default.MtServicePort;
            this.HttpServicePortBox = OpusCatMTEngineSettings.Default.HttpMtServicePort;
            this.StoreDataInAppdata = OpusCatMTEngineSettings.Default.StoreOpusCatDataInLocalAppdata;
            this.DatabaseRemovalInterval = OpusCatMTEngineSettings.Default.DatabaseRemovalInterval;
            this.UseDatabaseRemoval = OpusCatMTEngineSettings.Default.UseDatabaseRemoval;
            this.CacheMtInDatabase = OpusCatMTEngineSettings.Default.CacheMtInDatabase;
            NotifyPropertyChanged("SaveButtonEnabled");
        }


        private void OpenCustomSettingsInEditor_Click(object sender, RoutedEventArgs e)
        {
            var customizeYml = HelperFunctions.GetLocalAppDataPath(OpusCatMTEngineSettings.Default.CustomizationBaseConfig);
            Process.Start("notepad.exe", customizeYml);
        }


        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            OpusCatMTEngineSettings.Default.MtServicePort = this.ServicePortBox;
            OpusCatMTEngineSettings.Default.HttpMtServicePort = this.HttpServicePortBox;
            OpusCatMTEngineSettings.Default.StoreOpusCatDataInLocalAppdata = this.StoreDataInAppdata;
            OpusCatMTEngineSettings.Default.DatabaseRemovalInterval = this.DatabaseRemovalInterval;
            OpusCatMTEngineSettings.Default.UseDatabaseRemoval = this.UseDatabaseRemoval;
            OpusCatMTEngineSettings.Default.CacheMtInDatabase = this.CacheMtInDatabase;
            OpusCatMTEngineSettings.Default.Save();
            NotifyPropertyChanged("SaveButtonEnabled");
        }

        private void PreviewNumberInput(object sender, TextCompositionEventArgs e)
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

        private string databaseRemovalInterval;
        public string DatabaseRemovalInterval
        {
            get => databaseRemovalInterval;
            set
            {
                databaseRemovalInterval = value;
                NotifyPropertyChanged();
            }
        }

        public bool StoreDataInAppdata
        {
            get => _storeDataInAppdata;
            set
            {
                _storeDataInAppdata = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged("SaveButtonEnabled");
            }
        }

        public bool CacheMtInDatabase
        {
            get => _cacheMtInDatabase;
            set
            {
                _cacheMtInDatabase = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged("SaveButtonEnabled");
            }
        }

        public bool UseDatabaseRemoval
        {
            get => useDatabaseRemoval;
            set
            {
                useDatabaseRemoval = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged("SaveButtonEnabled");
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
            get
            {
                
                bool allSettingsDefault =
                    this.ServicePortBox == OpusCatMTEngineSettings.Default.MtServicePort &&
                    this.HttpServicePortBox == OpusCatMTEngineSettings.Default.HttpMtServicePort &&
                    this.StoreDataInAppdata == OpusCatMTEngineSettings.Default.StoreOpusCatDataInLocalAppdata &&
                    this.DatabaseRemovalInterval == OpusCatMTEngineSettings.Default.DatabaseRemovalInterval &&
                    this.UseDatabaseRemoval == OpusCatMTEngineSettings.Default.UseDatabaseRemoval &&
                    this.CacheMtInDatabase == OpusCatMTEngineSettings.Default.CacheMtInDatabase;
                
                return !allSettingsDefault && !this.validationErrors;
            }
        }
        
        private bool _storeDataInAppdata;
        private bool httpServicePortBoxIsValid;
        private bool servicePortBoxIsValid;
        private bool _cacheMtInDatabase;
        private bool validationErrors;
        private bool useDatabaseRemoval;

        public string Error
        {
            get { return "...."; }
        }


        private string Validate(string propertyName)
        {
            // Return error message if there is error on else return empty or null string
            string validationMessage = string.Empty;
            this.validationErrors = false;
            switch (propertyName)
            {
                case "DatabaseRemovalInterval":
                    if (!String.IsNullOrEmpty(this.DatabaseRemovalInterval))
                    {
                        var interval = Int32.Parse(this.DatabaseRemovalInterval);
                        if (interval == 0)
                        {
                            validationMessage = "Error";
                            this.validationErrors = true;
                        }
                    }
                    else
                    {
                        validationMessage = "Error";
                        this.validationErrors = true;
                    }
                    break;
                case "ServicePortBox":
                    if (this.ServicePortBox != null && this.ServicePortBox != "")
                    {
                        var portNumber = Int32.Parse(this.ServicePortBox);
                        if (portNumber < 1024 || portNumber > 65535)
                        {
                            validationMessage = "Error";
                            this.validationErrors = true;
                        }
                    }
                    else
                    {
                        validationMessage = "Error";
                        this.validationErrors = true;
                    }

                    break;
                case "HttpServicePortBox":
                    if (this.HttpServicePortBox != null && this.HttpServicePortBox != "")
                    {
                        var portNumber = Int32.Parse(this.HttpServicePortBox);
                        if (portNumber < 1024 || portNumber > 65535)
                        {
                            validationMessage = "Error";
                            this.validationErrors = true;
                        }
                        else
                        {
                            this.httpServicePortBoxIsValid = true;
                        }
                    }
                    else
                    {
                        validationMessage = "Error";
                        this.validationErrors = true;
                    }

                    break;
            }

            NotifyPropertyChanged("SaveButtonEnabled");
            return validationMessage;
        }

        private void revertToDefaultsButton_Click(object sender, RoutedEventArgs e)
        {
            OpusCatMTEngineSettings.Default.Reset();
            OpusCatMTEngineSettings.Default.Save();
            this.ServicePortBox = OpusCatMTEngineSettings.Default.MtServicePort;
            this.HttpServicePortBox = OpusCatMTEngineSettings.Default.HttpMtServicePort;
            this.StoreDataInAppdata = OpusCatMTEngineSettings.Default.StoreOpusCatDataInLocalAppdata;
            this.DatabaseRemovalInterval = OpusCatMTEngineSettings.Default.DatabaseRemovalInterval;
            this.UseDatabaseRemoval = OpusCatMTEngineSettings.Default.UseDatabaseRemoval;
            this.CacheMtInDatabase = OpusCatMTEngineSettings.Default.CacheMtInDatabase;
        }
    }
}
