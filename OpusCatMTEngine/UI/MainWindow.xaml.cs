using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
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

namespace OpusCatMTEngine
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public string WindowTitle
        {
            get
            {
                Version version = Assembly.GetExecutingAssembly().GetName().Version;
                return String.Format(OpusCatMTEngine.Properties.Resources.OpusCatWindowTitle, version);
            }
        }

        public ModelManager ModelManager { get; private set; }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (this.ModelManager.FinetuningOngoing || this.ModelManager.BatchTranslationOngoing)
            {
                MessageBoxResult result = MessageBox.Show(OpusCatMTEngine.Properties.Resources.ConfirmExitMessage,
                                          OpusCatMTEngine.Properties.Resources.ConfirmExitCaption,
                                          MessageBoxButton.YesNo,
                                          MessageBoxImage.Question);
                if (result != MessageBoxResult.Yes)
                {
                    e.Cancel = true;
                }
            }
        }


        internal void AddTab(ActionTabItem actionTabItem)
        {
            this.UiTabs.Add(actionTabItem);
            this.Tabs.SelectedItem = actionTabItem;
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
        

        public MainWindow()
        {
            Log.Information("Starting OPUS-CAT MT Engine");
            
            this.StartEngine();
            InitializeComponent();
        }

        private void StartEngine()
        {
            var service = new Service();
            this.ModelManager = new ModelManager();

            this.UiTabs = new ObservableCollection<ActionTabItem>();
            var localModels = new LocalModelListView(this.ModelManager);
            var settings = new OpusCatSettingsView();
            this.UiTabs.Add(
                new ActionTabItem { Content = localModels, Header = OpusCatMTEngine.Properties.Resources.ModelsTabTitle, Closable = false });
            this.UiTabs.Add(
                new ActionTabItem { Content = settings, Header = OpusCatMTEngine.Properties.Resources.SettingsTabTitle, Closable = false });

            this.DataContext = this;
            this.serviceHost = service.StartService(this.ModelManager);
        }
        
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.serviceHost.Close();
        }

        public ObservableCollection<ActionTabItem> UiTabs { get; set; }

        private void Image_MouseUp(object sender, MouseButtonEventArgs e)
        {
            this.UiTabs.RemoveAt(Tabs.SelectedIndex);
        }
    }
}
