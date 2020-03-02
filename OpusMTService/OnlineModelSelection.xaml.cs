using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;
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

namespace OpusMTService
{
    /// <summary>
    /// Interaction logic for ListBoxWithControls.xaml
    /// </summary>
    public partial class OnlineModelSelection: Window
    {
        private string sourceFilter = "";
        private string targetFilter = "";
        private string nameFilter = "";
        private ModelManager modelManager;

        public OnlineModelSelection()
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
            model.InstallStatus = "Installed";
            this.modelManager.ExtractModel(model.Path);
            this.modelManager.GetLocalModels();
        }

        private void btnInstall_Click(object sender, RoutedEventArgs e)
        {
            foreach (object selected in this.ModelListView.SelectedItems)
            {
                MTModel selectedModel = (MTModel)selected;
                selectedModel.InstallStatus = "Downloading";
                this.modelManager.DownloadModel(
                    selectedModel.Path,
                    selectedModel.DownloadProgressChanged,
                    (x,y) => DownloadCompleted(selectedModel,x,y));
            }
            
        }
        
        private void LbTodoList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void nameFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.nameFilter = ((TextBox)sender).Text;
            this.modelManager.FilterOnlineModels(this.sourceFilter, this.targetFilter, this.nameFilter);
        }

        private void sourceLangFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.sourceFilter = ((TextBox)sender).Text;
            this.modelManager.FilterOnlineModels(this.sourceFilter,this.targetFilter,this.nameFilter);
        }

        private void targetLangFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.targetFilter = ((TextBox)sender).Text;
            this.modelManager.FilterOnlineModels(this.sourceFilter, this.targetFilter, this.nameFilter);
        }
    }

}
