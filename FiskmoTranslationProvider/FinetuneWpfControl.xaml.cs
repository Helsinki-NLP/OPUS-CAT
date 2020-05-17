using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.ServiceModel;
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

namespace FiskmoTranslationProvider
{
    /// <summary>
    /// Interaction logic for FinetuneWpfControl.xaml
    /// </summary>
    public partial class FinetuneWpfControl : UserControl, INotifyPropertyChanged
    {
        private FinetuneBatchTaskSettings settings;
        public FinetuneBatchTaskSettings Settings 
        { 
            get
            {
                return settings;
            }
            set
            {
                settings = value;
                settings.MtServicePort = settings.MtServicePort;
                settings.MtServiceAddress = settings.MtServicePort;
                FetchServiceData();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void ServicePortBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private ObservableCollection<string> allModelTags;
        private string mtServiceAddress;
        private string mtServicePort;
        private string connectionStatus;
        private string modelTag;

        public ObservableCollection<string> AllModelTags { get => allModelTags; set { allModelTags = value; NotifyPropertyChanged(); } }

        private void FetchServiceData()
        {
            string connectionResult;
            try
            {
                var serviceLanguagePairs = FiskmöMTServiceHelper.ListSupportedLanguages(this.mtServiceAddress,this.mtServicePort);

                connectionResult = $"Available language pairs: {String.Join(", ", serviceLanguagePairs)}";
                
                //Get a list of model tags that are supported for these language pairs
                List<string> modelTags = new List<string>();
                foreach (var languagePair in serviceLanguagePairs)
                {
                    modelTags.AddRange(FiskmöMTServiceHelper.GetLanguagePairModelTags(this.Settings.MtServiceAddress, this.Settings.MtServicePort, languagePair.ToString()));
                }

                Dispatcher.Invoke(() => UpdateModelTags(modelTags));
            }
            catch (Exception ex) when (ex is EndpointNotFoundException || ex is CommunicationObjectFaultedException || ex is UriFormatException)
            {
                connectionResult = $"No connection to Fiskmö MT service at {this.mtServiceAddress}:{this.mtServicePort}.";
            }

            Dispatcher.Invoke(() => this.ConnectionStatus = connectionResult);
        }

        private void UpdateModelTags(List<string> tags)
        {
            this.AllModelTags.Clear();

            foreach (var tag in tags)
            {
                this.AllModelTags.Add(tag);
            }

        }

        public FinetuneWpfControl()
        {
            InitializeComponent();
        }

        public string ConnectionStatus
        {
            get => connectionStatus;
            set
            {
                connectionStatus = value;
                NotifyPropertyChanged();
            }
        }


        private void ModeButton_Checked(object sender, RoutedEventArgs e)
        {
            var radioButton = ((RadioButton)sender);
            if (radioButton.IsChecked.Value)
            {
                switch (radioButton.Name)
                {
                    case "FinetuneAndTranslate":
                        this.Settings.BatchTranslate = true;
                        this.Settings.Finetune = true;
                        break;
                    case "FinetuneOnly:":
                        this.Settings.Finetune = true;
                        this.Settings.BatchTranslate = false;
                        break;
                    case "TranslateOnly":
                        this.Settings.Finetune = false;
                        this.Settings.BatchTranslate = true;
                        break;
                }
            }
        }

        private void RetryConnection_Click(object sender, RoutedEventArgs e)
        {
            this.FetchServiceData();
        }
    }
}
