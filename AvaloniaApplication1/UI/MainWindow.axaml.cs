using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Input;
using OpusCatMtEngine;
using Serilog;
using System;
using System.Reflection;
using System.Linq;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia;
using Avalonia.Controls.ApplicationLifetimes;

namespace OpusCatMtEngine
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            Log.Information("Starting OPUS-CAT MT Engine");
          
            this.ValidatePath();

            this.StartEngine();
            InitializeComponent();
            
            this.Opened += MainWindow_Loaded;
            
        }

        
    

        public string WindowTitle
        {
            get
            {
                Version version = Assembly.GetExecutingAssembly().GetName().Version;
                return String.Format(OpusCatMtEngine.Properties.Resources.Main_OpusCatWindowTitle, version);
            }
        }

        public ModelManager? ModelManager { get; private set; }

        
        //TODO: make this work in Avalonia, maybe wait until Mac implementation to avoid
        //wasted effort (apparently this works differently there)
        /*protected override void OnClosing(CancelEventArgs e)
        {
            if (this.ModelManager.FinetuningOngoing || this.ModelManager.BatchTranslationOngoing)
            {
                MessageBoxResult result = MessageBox.Show(Properties.Resources.Main_ConfirmExitMessage,
                                          Properties.Resources.Main_ConfirmExitCaption,
                                          MessageBoxButton.YesNo,
                                          MessageBoxImage.Question);
                if (result != MessageBoxResult.Yes)
                {
                    e.Cancel = true;
                }
            }
        }*/


        internal void AddTab(ActionTabItem actionTabItem)
        {
            this.UiTabs.Add(actionTabItem);
            this.Tabs.SelectedItem = actionTabItem;
        }



        public event PropertyChangedEventHandler? PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private async void ValidatePath()
        {
            //If the model directory path contains non-ascii characters, Marian will not
            //work properly. Should recompile Marian (or preferably switch to
            //Bergamot version) to fix this, but needs a quick workaround.
            var opuscatDataPath = HelperFunctions.GetOpusCatDataPath();
            var illegalCharacters = opuscatDataPath.Where(x => x > 127);



            if (illegalCharacters.Any())
            {
                //Try to circumvent by using a opuscat dir in the installation folder
                if (OpusCatMtEngineSettings.Default.StoreOpusCatDataInLocalAppdata)
                {
                    OpusCatMtEngineSettings.Default.StoreOpusCatDataInLocalAppdata = false;
                    OpusCatMtEngineSettings.Default.Save();
                    OpusCatMtEngineSettings.Default.Reload();

                    //refetch the path
                    opuscatDataPath = HelperFunctions.GetOpusCatDataPath();
                    illegalCharacters = opuscatDataPath.Where(x => x > 127);
                }

                var box = MessageBoxManager.GetMessageBoxStandard(
                    OpusCatMtEngine.Properties.Resources.Main_NonAsciiPathCaption,
                    String.Format(
                            OpusCatMtEngine.Properties.Resources.Main_NonAsciiPathMessage,
                            opuscatDataPath,
                            String.Join(",", illegalCharacters)),
                            ButtonEnum.Ok,
                            MsBox.Avalonia.Enums.Icon.Warning);

                await box.ShowAsync();

                //TODO: check this actually works
                if (Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime lifetime)
                {
                    lifetime.Shutdown();
                }
            }
        }

        

        private void MainWindow_Loaded(object? sender, EventArgs e)
        {
            this.Loaded -= MainWindow_Loaded;
            if (OpusCatMtEngineSettings.Default.CacheMtInDatabase)
            {
                TranslationDbHelper.SetupTranslationDb();
            }
        }

        private void MainWindow_Loaded(object? sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void StartEngine()
        {

            this.ModelManager = new ModelManager();

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


        public ObservableCollection<ActionTabItem>? UiTabs { get; set; }

        
        private void Image_PointerReleased(object sender, PointerReleasedEventArgs e)
        {
            this.UiTabs.RemoveAt(Tabs.SelectedIndex);
        }
    }
}