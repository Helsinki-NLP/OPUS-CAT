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
    public partial class ConnectionControl : UserControl,INotifyPropertyChanged
    {
        private string connectionStatus;
        private ObservableCollection<string> allModelTags;

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
            string connectionResult;
            try
            {
                var serviceLanguagePairs = FiskmöMTServiceHelper.ListSupportedLanguages(this.ServicePortBoxElement.Text, this.ServiceAddressBoxElement.Text);

                connectionResult = $"Available language pairs: {String.Join(", ", serviceLanguagePairs)}";

                //Get a list of model tags that are supported for these language pairs
                List<string> modelTags = new List<string>();
                foreach (var languagePair in serviceLanguagePairs)
                {
                    modelTags.AddRange(FiskmöMTServiceHelper.GetLanguagePairModelTags(this.ServicePortBoxElement.Text, this.ServiceAddressBoxElement.Text, languagePair.ToString()));
                }

                Dispatcher.Invoke(() => UpdateModelTags(modelTags));
            }
            catch (Exception ex) when (ex is EndpointNotFoundException || ex is CommunicationObjectFaultedException || ex is UriFormatException)
            {
                connectionResult = $"No connection to Fiskmö MT service at {this.ServicePortBoxElement.Text}:{this.ServiceAddressBoxElement.Text}.";
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

        public ConnectionControl()
        {
            //This will be overwritten with the options passed as datacontext, but assign
            //this in the meantime to prevent null exceptions.
            //this.Options = new FiskmoOptions();
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

    }
}
