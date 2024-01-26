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

namespace OpusCatMtEngine
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
                return String.Format(OpusCatMtEngine.Properties.Resources.Main_OpusCatWindowTitle, version);
            }
        }

        public ModelManager ModelManager { get; private set; }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (this.ModelManager.FinetuningOngoing || this.ModelManager.BatchTranslationOngoing)
            {
                MessageBoxResult result = MessageBox.Show(OpusCatMtEngine.Properties.Resources.Main_ConfirmExitMessage,
                                          OpusCatMtEngine.Properties.Resources.Main_ConfirmExitCaption,
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
        
        private void ValidatePath()
        {
            //If the model directory path contains non-ascii characters, Marian will not
            //work properly. Should recompile Marian (or preferably switch to
            //Bergamot version) to fix this, but needs a quick workaround.
            var opuscatDataPath = HelperFunctions.GetOpusCatDataPath();
            var illegalCharacters = opuscatDataPath.Where(x => x > 127);

            

            if (illegalCharacters.Any())
            {
                //Try to circumvent by using a opuscat dir in the installation folder
                if (OpusCatMTEngineSettings.Default.StoreOpusCatDataInLocalAppdata)
                {
                    OpusCatMTEngineSettings.Default.StoreOpusCatDataInLocalAppdata = false;
                    OpusCatMTEngineSettings.Default.Save();
                    OpusCatMTEngineSettings.Default.Reload();

                    //refetch the path
                    opuscatDataPath = HelperFunctions.GetOpusCatDataPath();
                    illegalCharacters = opuscatDataPath.Where(x => x > 127);
                }


                MessageBoxResult result =
                    MessageBox.Show(
                        String.Format(
                            OpusCatMtEngine.Properties.Resources.Main_NonAsciiPathMessage,
                            opuscatDataPath,
                            String.Join(",", illegalCharacters)),
                        OpusCatMtEngine.Properties.Resources.Main_NonAsciiPathCaption,
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                System.Windows.Application.Current.Shutdown();
            }
        }

        public MainWindow()
        {
            Log.Information("Starting OPUS-CAT MT Engine");

            this.ValidatePath();
            
            this.StartEngine();
            InitializeComponent();

            this.Loaded += MainWindow_Loaded;
            
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= MainWindow_Loaded;
            if (OpusCatMTEngineSettings.Default.CacheMtInDatabase)
            {
                TranslationDbHelper.SetupTranslationDb();
            }
        }

        private void StartEngine()
        {
            
            this.ModelManager = new ModelManager();

            //The WCF selfhost implementation
            var service = new Service();
            this.serviceHost = service.StartService(this.ModelManager);

            //OWIN selfhost implementation
            var owin = new OwinMtService(this.ModelManager);

            this.UiTabs = new ObservableCollection<ActionTabItem>();
            var localModels = new LocalModelListView(this.ModelManager);
            var settings = new OpusCatSettingsView();
            this.UiTabs.Add(
                new ActionTabItem { Content = localModels, Header = OpusCatMtEngine.Properties.Resources.Main_ModelsTabTitle, Closable = false });
            this.UiTabs.Add(
                new ActionTabItem { Content = settings, Header = OpusCatMtEngine.Properties.Resources.Main_SettingsTabTitle, Closable = false });

            this.DataContext = this;
            
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
