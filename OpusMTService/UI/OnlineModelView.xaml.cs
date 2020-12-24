using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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
using System.Windows.Shapes;

namespace OpusCatMTEngine
{
    /// <summary>
    /// Interaction logic for OnlineModelView.xaml
    /// </summary>
    public partial class OnlineModelView : UserControl
    {
        private string sourceFilter = "";
        private string targetFilter = "";
        private string nameFilter = "";
        private ModelManager modelManager;
        private GridViewColumnHeader lastHeaderClicked;
        private ListSortDirection lastDirection;

        public OnlineModelView()
        {
            InitializeComponent();
            this.DataContextChanged += dataContextChanged;
        }

        private void dataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            this.modelManager = ((ModelManager)this.DataContext);
        }

        internal void DownloadCompleted(MTModel model, object sender, AsyncCompletedEventArgs e)
        {
            model.InstallStatus = "Extracting";
            this.modelManager.ExtractModel(model.ModelPath);
            model.InstallStatus = "Installed";
            this.modelManager.GetLocalModels();
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

                selectedModel.InstallStatus = "Downloading";
                this.modelManager.DownloadModel(
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
            if (this.modelManager != null)
            {
                this.modelManager.FilterOnlineModels(this.sourceFilter, this.targetFilter, this.nameFilter);
            }
        }

        private void sourceLangFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.sourceFilter = ((TextBox)sender).Text;
            if (this.modelManager != null)
            {
                this.modelManager.FilterOnlineModels(this.sourceFilter, this.targetFilter, this.nameFilter);
            }
        }

        private void targetLangFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.targetFilter = ((TextBox)sender).Text;
            if (this.modelManager != null)
            {
                this.modelManager.FilterOnlineModels(this.sourceFilter, this.targetFilter, this.nameFilter);
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

                    var columnBinding = headerClicked.Column.DisplayMemberBinding as Binding;
                    var sortBy = columnBinding?.Path.Path ?? headerClicked.Column.Header as string;

                    if (sortBy == "Installation progress")
                    {
                        sortBy = "InstallProgress";
                    }

                    this.modelManager.SortOnlineModels(sortBy, direction);

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
