using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using YamlDotNet.Serialization;

namespace OpusCatMTEngine
{
    /// <summary>
    /// Interaction logic for OnlineModelView.xaml
    /// </summary>
    public partial class OnlineModelView : UserControl, INotifyPropertyChanged
    {
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private string sourceFilter = "";
        private string targetFilter = "";
        private string nameFilter = "";
        private ModelManager modelManager;
        private GridViewColumnHeader lastHeaderClicked;
        private ListSortDirection lastDirection;

        private bool showBilingualModels;
        public bool ShowBilingualModels
        {
            get => showBilingualModels;
            set
            {
                showBilingualModels = value;
                NotifyPropertyChanged();
                this.FilterModels();
            }
        }

        private bool showMultilingualModels;
        private bool _showOpusModels;
        private bool _showTatoebaModels;

        public bool ShowMultilingualModels
        {
            get => showMultilingualModels;
            set
            {
                showMultilingualModels = value;
                NotifyPropertyChanged();
                this.FilterModels();
            }
        }

        public bool ShowOpusModels
        {
            get => _showOpusModels;
            set
            {
                _showOpusModels = value;
                NotifyPropertyChanged();
                this.FilterModels();
            }
        }

        public bool ShowTatoebaModels
        {
            get => _showTatoebaModels;
            set
            {
                _showTatoebaModels = value;
                NotifyPropertyChanged();
                this.FilterModels();
            }
        }

        public ModelManager ModelManager { get => modelManager; set => modelManager = value; }

        public OnlineModelView()
        {
            InitializeComponent();
            this.DataContextChanged += dataContextChanged;
        }

        private void dataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            this.ModelManager = ((ModelManager)this.DataContext);
            this.ModelManager.GetOnlineModels();
            this.ModelManager.FilteredOnlineModels.CollectionChanged += FilteredOnlineModels_CollectionChanged;
            NotifyPropertyChanged(null);
        }

        private void FilteredOnlineModels_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            this.ModelCount.GetBindingExpression(Label.ContentProperty).UpdateTarget();
        }

        internal void DownloadCompleted(MTModel model, object sender, AsyncCompletedEventArgs e)
        {
            model.InstallStatus = OpusCatMTEngine.Properties.Resources.Online_ExtractingStatus;
            var installPath = this.ModelManager.ExtractModel(model.ModelPath,true);
            
            //If model has yaml config, check whether it was included in the zip package (Tatoeba models)
            if (!String.IsNullOrEmpty(model.TatoebaConfigString))
            {
                var decoderYaml = 
                    new DirectoryInfo(installPath).GetFiles("decoder.yml").Single();
                var deserializer = new Deserializer();
                var decoderSettings = deserializer.Deserialize<MarianDecoderConfig>(decoderYaml.OpenText());
                var modelPath = Path.Combine(installPath, decoderSettings.models[0]);
                var yamlPath = Path.ChangeExtension(modelPath, "yml");

                //The yamls inside the model zips may be corrupt, so always write the config string as yaml,
                //as that is more current.
                using (var writer = File.CreateText(yamlPath))
                {
                    writer.Write(model.TatoebaConfigString);
                }
            }

            model.InstallStatus = OpusCatMTEngine.Properties.Resources.Online_InstalledStatus;
            this.ModelManager.GetLocalModels();
        }

        private void btnInstall_Click(object sender, RoutedEventArgs e)
        {
            foreach (object selected in this.ModelListView.SelectedItems)
            {
                MTModel selectedModel = (MTModel)selected;
                if (selectedModel.InstallStatus != "")
                {
                    continue;
                }

                selectedModel.InstallStatus = OpusCatMTEngine.Properties.Resources.Online_DownloadingStatus;
                this.ModelManager.DownloadModel(
                    selectedModel.ModelUri,
                    selectedModel.ModelPath,
                    selectedModel.DownloadProgressChanged,
                    (x, y) => DownloadCompleted(selectedModel, x, y));
            }

        }

        private void LbTodoList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void nameFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.nameFilter = ((TextBox)sender).Text;
            if (this.ModelManager != null)
            {
                this.FilterModels();
            }
        }

        private void FilterModels()
        {
            this.ModelManager.FilterOnlineModels();
        }

        private void sourceLangFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.sourceFilter = ((TextBox)sender).Text;
            if (this.ModelManager != null)
            {
                this.FilterModels();
            }
        }

        private void targetLangFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.targetFilter = ((TextBox)sender).Text;
            if (this.ModelManager != null)
            {
                this.FilterModels();
            }
        }

        private void GridViewColumnHeaderClickedHandler(object sender, RoutedEventArgs e)
        {
            var headerClicked = e.OriginalSource as GridViewColumnHeader;
            ListSortDirection direction;

            if (headerClicked != null)
            {
                if (headerClicked.Role != GridViewColumnHeaderRole.Padding)
                {
                    if (headerClicked != this.lastHeaderClicked)
                    {
                        direction = ListSortDirection.Ascending;
                    }
                    else
                    {
                        if (this.lastDirection == ListSortDirection.Ascending)
                        {
                            direction = ListSortDirection.Descending;
                        }
                        else
                        {
                            direction = ListSortDirection.Ascending;
                        }
                    }

                    //DisplayMemberBinding is only available for some columns, not for e.g. those
                    //with DataTemplates for line wrapping etc. Those need to be handled by the switch
                    //block below
                    var columnBinding = headerClicked.Column.DisplayMemberBinding as Binding;
                    var sortBy = columnBinding?.Path.Path ?? headerClicked.Column.Header as string;

                    switch (sortBy)
                    {
                        case "Installation progress":
                            sortBy = "InstallProgress";
                            break;
                        case "Source languages":
                            sortBy = "SourceLanguageString";
                            break;
                        case "Target languages":
                            sortBy = "TargetLanguageString";
                            break;
                    }

                    this.ModelManager.SortOnlineModels(sortBy, direction);

                    if (direction == ListSortDirection.Ascending)
                    {
                        headerClicked.Column.HeaderTemplate =
                          Resources["HeaderTemplateArrowUp"] as DataTemplate;
                    }
                    else
                    {
                        headerClicked.Column.HeaderTemplate =
                          Resources["HeaderTemplateArrowDown"] as DataTemplate;
                    }

                    // Remove arrow from previously sorted header
                    if (this.lastHeaderClicked != null && this.lastHeaderClicked != headerClicked)
                    {
                        this.lastHeaderClicked.Column.HeaderTemplate = null;
                    }

                    this.lastHeaderClicked = headerClicked;
                    this.lastDirection = direction;
                }
            }
        }
    }
}
