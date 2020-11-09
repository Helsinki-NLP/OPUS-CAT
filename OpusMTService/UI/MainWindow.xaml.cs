using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
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

namespace FiskmoMTEngine
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IDataErrorInfo, INotifyPropertyChanged
    {
        public ModelManager ModelManager { get; private set; }

        private bool saveButtonEnabled;

        public string Error
        {
            get { return "...."; }
        }

        public string ServicePortBox
        {
            get => servicePortBox;
            set {
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

        public string this[string columnName]
        {
            get
            {
                return Validate(columnName);
            }
        }

        private string Validate(string propertyName)
        {
            // Return error message if there is error on else return empty or null string
            string validationMessage = string.Empty;
            this.SaveButtonEnabled = false;
            switch (propertyName)
            {
                case "ServicePortBox":
                    if (this.ServicePortBox != null && this.ServicePortBox != "" )
                    {
                        var portNumber = Int32.Parse(this.ServicePortBox);
                        if (portNumber < 1024 || portNumber > 65535)
                        {
                            validationMessage = "Error";
                        }
                        else
                        {
                            if (this.ServicePortBox != FiskmoMTEngineSettings.Default.MtServicePort)
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

        private ServiceHost serviceHost;
        private string servicePortBox;

        public MainWindow()
        {
            Log.Information("Starting Fiskmö MT service");
            
            this.StartEngine();
            InitializeComponent();
            this.ServicePortBox = FiskmoMTEngineSettings.Default.MtServicePort;
        }

        private void StartEngine()
        {
            var service = new Service();
            this.ModelManager = new ModelManager();
            
            this.UiTabs = new ObservableCollection<ActionTabItem>();
            var localModels = new LocalModelListView(this.ModelManager);
            this.UiTabs.Add(new ActionTabItem { Content = localModels, Header = "Models" });

            this.DataContext = this;
            this.serviceHost = service.StartService(this.ModelManager);
        }

        private void restartButton_Click(object sender, RoutedEventArgs e)
        {
            //Abort the service and start it again
            this.serviceHost.Abort();
            this.StartEngine();
        }

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            FiskmoMTEngineSettings.Default.MtServicePort = this.ServicePortBox;
            FiskmoMTEngineSettings.Default.Save();
            this.SaveButtonEnabled = false;
        }

        private void ServicePortBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.serviceHost.Close();
        }

        public ObservableCollection<ActionTabItem> UiTabs { get; set; }

        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }
    }
}
