using Sdl.LanguagePlatform.Core;
using Sdl.ProjectAutomation.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
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
    /// Interaction logic for FiskmoOptions.xaml
    /// </summary>
    public partial class FiskmoOptionControl : UserControl, IDataErrorInfo, INotifyPropertyChanged
    {
        public string this[string columnName]
        {
            get
            {
                return Validate(columnName);
            }
        }

        private ObservableCollection<string> allModelTags;

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
                var serviceLanguagePairs = FiskmöMTServiceHelper.ListSupportedLanguages(this.options);

                

                var projectLanguagePairsWithMt = serviceLanguagePairs.Intersect(this.projectLanguagePairs);
                if (projectLanguagePairsWithMt.Count() == 0)
                {
                    connectionResult = "No MT models available for the language pairs of the project";
                }
                else if (projectLanguagePairs.Count == projectLanguagePairsWithMt.Count())
                {
                    connectionResult = "MT models available for all the language pairs of the project";
                }
                else
                {
                    connectionResult = $"MT models available for some of the language pairs of the project: {String.Join(", ", projectLanguagePairsWithMt)}";
                }

                //Get a list of model tags that are supported for these language pairs
                List<string> modelTags = new List<string>();
                foreach (var languagePair in this.projectLanguagePairs)
                {
                    modelTags.AddRange(FiskmöMTServiceHelper.GetLanguagePairModelTags(this.options, languagePair.ToString()));
                }

                Dispatcher.Invoke(() => UpdateModelTags(modelTags));
            }
            catch (Exception ex) when (ex is EndpointNotFoundException || ex is CommunicationObjectFaultedException)
            {
                connectionResult = $"No connection to Fiskmö MT service at {this.options.mtServiceAddress}:{this.options.mtServicePort}.";
            }

            Dispatcher.Invoke(() => this.ConnectionStatus = connectionResult);
        }

        private void UpdateModelTags(List<string> tags)
        {
            this.AllModelTags.Clear();

            //Always add the model tag from options, if present, and select it
            if (this.options.modelTag != "" && this.options.modelTag != null)
            {
                this.AllModelTags.Add(this.options.modelTag);
                this.TagBox.SelectedIndex = 0;
            }

            foreach (var tag in tags)
            {
                this.AllModelTags.Add(tag);    
            }
            
        }

        public FiskmoOptionControl(FiskmoOptionsFormWPF hostForm, FiskmoOptions options, Sdl.LanguagePlatform.Core.LanguagePair[] languagePairs)
        {
            this.DataContext = this;
            this.AllModelTags = new ObservableCollection<string>();
            this.options = options;
            this.projectLanguagePairs = languagePairs.Select(
                x => $"{x.SourceCulture.TwoLetterISOLanguageName}-{x.TargetCulture.TwoLetterISOLanguageName}").ToList();

            //Update model tag list with potential model tag from options
            this.UpdateModelTags(new List<string>());

            //Check whether there's a connection to a MT service

            System.Threading.Tasks.Task.Run(this.FetchServiceData);

            InitializeComponent();

            //Null indicates that all properties have changed. Populates the WPF form
            PropertyChanged(this, new PropertyChangedEventArgs(null));

            this.hostForm = hostForm;
        }

        private string Validate(string propertyName)
        {
            // Return error message if there is error on else return empty or null string
            string validationMessage = string.Empty;
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
                    }
                    else
                    {
                        validationMessage = "Error";
                    }

                    break;
            }

            return validationMessage;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private FiskmoOptions options;
        private List<string> projectLanguagePairs;
        private FiskmoOptionsFormWPF hostForm;
        private string connectionStatus;

        public string ServicePortBox
        {
            get => this.options.mtServicePort;
            set
            {
                this.options.mtServicePort = value;
                NotifyPropertyChanged();
            }
        }

        public string ServiceAddressBox
        {
            get => this.options.mtServiceAddress;
            set
            {
                this.options.mtServiceAddress = value;
                NotifyPropertyChanged();
            }
        }



        public Boolean PregenerateMt
        {
            get => this.options.pregenerateMt;
            set
            {
                this.options.pregenerateMt = value;
                NotifyPropertyChanged();
            }
        }
        public Boolean ShowMtAsOrigin
        {
            get => this.options.showMtAsOrigin;
            set
            {
                this.options.showMtAsOrigin = value;
                NotifyPropertyChanged();
            }
        }


        public Boolean IncludeTagsAsText
        {
            get => this.options.includePlaceholderTags;
            set
            {
                this.options.includePlaceholderTags = value;
                NotifyPropertyChanged();
            }
        }
        

        public string Error
        {
            get { return "...."; }
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

        private void cancel_Click(object sender, RoutedEventArgs e)
        {
            this.hostForm.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        }

        private void save_Click(object sender, RoutedEventArgs e)
        {
            this.hostForm.DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        private void RetryConnection_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
