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

namespace OpusCatTranslationProvider
{
    /// <summary>
    /// Interaction logic for ConnectionControl.xaml
    /// </summary>
    public partial class ConnectionControl : UserControl, INotifyPropertyChanged
    {
        private string connectionStatus;
        private ObservableCollection<string> allModelTags;
        private bool noConnection;
        private OpusCatOptions options;
        internal static List<string> MtServiceLanguagePairs;

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

        private void FetchServiceData(string host, string port, string modeltag)
        {

            StringBuilder connectionResult = new StringBuilder();
            try
            {
                if (OpusCatProvider.OpusCatMtEngineConnection == null)
                {
                    OpusCatProvider.OpusCatMtEngineConnection = new OpusCatMtServiceConnection();
                }
                ConnectionControl.MtServiceLanguagePairs = OpusCatProvider.OpusCatMtEngineConnection.ListSupportedLanguages(host,port);
                IEnumerable<string> modelTagLanguagePairs;
                if (this.LanguagePairs != null)
                {
                    var projectLanguagePairsWithMt = ConnectionControl.MtServiceLanguagePairs.Intersect(this.LanguagePairs);
                    modelTagLanguagePairs = projectLanguagePairsWithMt;
                    if (projectLanguagePairsWithMt.Count() == 0)
                    {
                        connectionResult.Append("No MT models available for the language pairs of the project");
                    }
                    else if (this.LanguagePairs.Count == projectLanguagePairsWithMt.Count())
                    {
                        connectionResult.Append("MT models available for all the language pairs of the project");
                    }
                    else
                    {
                        connectionResult.Append($"MT models available for some of the language pairs of the project: {String.Join(", ", projectLanguagePairsWithMt)}");
                    }

                    //Get the detailed status for each project language pair
                    foreach (var pair in this.LanguagePairs)
                    {
                        connectionResult.Append(Environment.NewLine);
                        var sourceTarget = pair.Split('-');
                        connectionResult.Append(OpusCatProvider.OpusCatMtEngineConnection.CheckModelStatus(host, port, sourceTarget[0], sourceTarget[1], modeltag));
                    }
                }
                else
                {
                    //This options is used with the batch task, where there's no easy way of getting
                    //the project language pairs, so all pairs are assumed.
                    modelTagLanguagePairs = ConnectionControl.MtServiceLanguagePairs;
                    connectionResult.Append($"MT models available for following language pairs: {String.Join(", ", ConnectionControl.MtServiceLanguagePairs)}");
                }
                
                //Get a list of model tags that are supported for these language pairs
                List<string> modelTags = new List<string>();
                foreach (var languagePair in modelTagLanguagePairs)
                {
                    var pairSplit = languagePair.Split('-');
                    modelTags.AddRange(OpusCatProvider.OpusCatMtEngineConnection.GetLanguagePairModelTags(host, port, pairSplit[0],pairSplit[1]));
                }

                this.NoConnection = false;

                Dispatcher.Invoke(() => UpdateModelTags(modelTags,modeltag));
            }
            catch (Exception ex) when (ex is OpusCatEngineConnectionException)
            {
                connectionResult.Append($"No connection to OPUS-CAT MT Engine at {host}:{port}."+Environment.NewLine);
                connectionResult.Append("Make sure OPUS-CAT MT Engine application has been installed on your computer(check help link below) and is running and that it uses the same connection settings as the plugin (default settings should work).");
                this.NoConnection = true;
            }

            Dispatcher.Invoke(() => this.ConnectionStatus = connectionResult.ToString());
        }

        internal void Refresh()
        {
            Dispatcher.BeginInvoke(new Action(StartFetch), System.Windows.Threading.DispatcherPriority.ContextIdle);
        }

        private void UpdateModelTags(List<string> tags, string currentModelTag)
        {
            //Include currently selected tag, if it's already not included
            /*if (!tags.Contains(currentModelTag))
            {
                tags.Add(currentModelTag);
            }*/

            foreach (var tag in new List<string>(this.AllModelTags))
            {
                if (!tags.Contains(tag))
                {
                    this.AllModelTags.Remove(tag);
                }
            }

            foreach (var tag in tags)
            {
                if (!this.AllModelTags.Contains(tag))
                {
                    this.AllModelTags.Add(tag);
                }
            }

            NotifyPropertyChanged("AllModelTags");

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

        public ObservableCollection<string> AllModelTags
        {
            get
            {
                return allModelTags;
            }
            set
            {
                allModelTags = value;
                NotifyPropertyChanged();
            }
        }

        public bool NoConnection
        {
            get
            {
                if (this.options.opusCatSource == OpusCatOptions.OpusCatSource.OpusCatMtEngine)
                {
                    return noConnection;
                }
                else
                {
                    return false;
                }
            }
            set
            {
                noConnection = value;
                NotifyPropertyChanged("NoConnection");
                NotifyPropertyChanged("ConnectionExists");
                if (value)
                {
                    Dispatcher.Invoke(
                        () => this.ConnectionColor = new RadialGradientBrush(Colors.Red,Colors.DarkRed));
                }
                else
                {
                    Dispatcher.Invoke(() => this.ConnectionColor = new RadialGradientBrush(Colors.LightGreen, Colors.Green));
                }
            }
        }

        public bool ConnectionExists
        {
            get => !noConnection;
        }

        public List<string> LanguagePairs { get; set; }
        public Brush ConnectionColor 
        { 
            get => connectionColor;
            set { connectionColor = value; NotifyPropertyChanged(); }
        }

        public void AddModelTag(string tag)
        {            
            //It's possible that the options contain a tag which is not present
            //at the service. Include that tag in the list, since the omission might
            //be due to the service not being up properly etc.
            if (tag != "" && tag != null)
            {
                if (!this.AllModelTags.Contains(tag))
                {
                    this.AllModelTags.Add(tag);
                }
            }
            
        }

        public ConnectionControl()
        {
            InitializeComponent();

            this.DataContextChanged += ConnectionControl_DataContextChanged;

            //Fetch data only after data context has been set and the bindings have been resolved.
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                this.Refresh();
                
            }
            
        }

        private void ConnectionControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is IHasOpusCatOptions)
            {

                this.options = ((IHasOpusCatOptions)e.NewValue).Options;
            }
            this.AllModelTags = new ObservableCollection<string>();
        }

        private void StartFetch()
        {
            //If ELG has been selected as source, don't do the fetch.
            if (this.options.opusCatSource == OpusCatOptions.OpusCatSource.Elg)
            {
                NotifyPropertyChanged("NoConnection");
                return;
            }

            var host = this.options.mtServiceAddress;
            var port = this.options.mtServicePort;
            this.ConnectionColor = new RadialGradientBrush(Colors.Yellow, Colors.DarkGoldenrod);
            this.ConnectionStatus = $"Connecting to OPUS-CAT MT Engine at {host}:{port}.";

            //If connection details are custom, check the custom checkbox, this is for start-up
            this.UseCustomConnection.IsChecked =
                host != OpusCatTpSettings.Default.MtServiceAddress ||
                port != OpusCatTpSettings.Default.MtServicePort;

            var modeltag = this.options.modelTag;
            Task.Run(() => this.FetchServiceData(host, port, modeltag));
        }

        private Brush connectionColor;

        private void RetryConnection_Click(object sender, RoutedEventArgs e)
        {
            this.StartFetch();
        }

        private void SaveAsDefault_Click(object sender, RoutedEventArgs e)
        {
            OpusCatTpSettings.Default.MtServicePort = this.ServicePortBoxElement.Text;
            OpusCatTpSettings.Default.MtServiceAddress = this.ServiceAddressBoxElement.Text;
            OpusCatTpSettings.Default.Save();
        }

        public void TagBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.ConnectionStatus = "";
            this.StartFetch();
        }

        private void UseCustomConnection_Unchecked(object sender, RoutedEventArgs e)
        {
            this.ServicePortBoxElement.Text = OpusCatTpSettings.Default.MtServicePort;
            this.ServiceAddressBoxElement.Text = OpusCatTpSettings.Default.MtServiceAddress;
        }
    }
}
