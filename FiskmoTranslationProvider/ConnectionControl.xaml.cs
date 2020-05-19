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
    /// Interaction logic for ConnectionControl.xaml
    /// </summary>
    public partial class ConnectionControl : UserControl, INotifyPropertyChanged
    {
        private string connectionStatus;
        private ObservableCollection<string> allModelTags;
        private bool connectionExists;

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }


        private void ServicePortBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void FetchServiceData()
        {
            var host = this.ServiceAddressBoxElement.Text;
            var port = this.ServicePortBoxElement.Text;
            string connectionResult;
            try
            {

                var serviceLanguagePairs = FiskmöMTServiceHelper.ListSupportedLanguages(host, port);

                connectionResult = $"Available language pairs: {String.Join(", ", serviceLanguagePairs)}";

                //Get a list of model tags that are supported for these language pairs
                List<string> modelTags = new List<string>();
                foreach (var languagePair in serviceLanguagePairs)
                {
                    modelTags.AddRange(FiskmöMTServiceHelper.GetLanguagePairModelTags(host, port, languagePair.ToString()));
                }

                NoConnection = false;

                Dispatcher.Invoke(() => UpdateModelTags(modelTags));
            }
            catch (Exception ex) when (ex is EndpointNotFoundException || ex is CommunicationObjectFaultedException || ex is UriFormatException)
            {
                connectionResult = $"No connection to Fiskmö MT service at {host}:{port}. Make sure that the Fiskmö MT Engine application is running on your computer. Click the link below to view detailed help (external web page).";
                NoConnection = true;
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

        public string ConnectionStatus
        {
            get => connectionStatus;
            set
            {
                connectionStatus = value;
                NotifyPropertyChanged();
            }
        }

        public ObservableCollection<string> AllModelTags { get => allModelTags; set { allModelTags = value; NotifyPropertyChanged(); } }

        public bool NoConnection
        {
            get => connectionExists;
            set
            {
                connectionExists = value; NotifyPropertyChanged();
            }
        }

        public ConnectionControl()
        {
            this.AllModelTags = new ObservableCollection<string>();
            this.DataContextChanged += ConnectionControl_DataContextChanged;
            InitializeComponent();

            //Fetch data only after data context has been set and the bindings have been resolved.
            Dispatcher.BeginInvoke(new Action(() => this.FetchServiceData()), System.Windows.Threading.DispatcherPriority.ContextIdle);
        }

        private void ConnectionControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
        }

        private void RetryConnection_Click(object sender, RoutedEventArgs e)
        {
            this.FetchServiceData();
        }

        private void SaveAsDefault_Click(object sender, RoutedEventArgs e)
        {
            FiskmoTpSettings.Default.MtServicePort = this.ServicePortBoxElement.Text;
            FiskmoTpSettings.Default.MtServiceAddress = this.ServiceAddressBoxElement.Text;
        }
    }
}
