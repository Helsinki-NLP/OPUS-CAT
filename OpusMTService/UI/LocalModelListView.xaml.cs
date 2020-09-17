using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
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

namespace FiskmoMTEngine
{
    /// <summary>
    /// Interaction logic for ListBoxWithControls.xaml
    /// </summary>
    public partial class LocalModelListView : UserControl
    {
        
        public LocalModelListView()
        {
            InitializeComponent();
            this.DataContextChanged += dataContextChanged;
        }

        private void dataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            
        }


        private void btnOpenModelDir_Click(object sender, RoutedEventArgs e)
        {
            var selectedModel = (MTModel)this.LocalModelList.SelectedItem;
            Process.Start(selectedModel.InstallDir);
        }


        private void btnAddOnlineModel_Click(object sender, RoutedEventArgs e)
        {
            OnlineModelSelection onlineSelection = new OnlineModelSelection();
            onlineSelection.DataContext = this.DataContext;
            onlineSelection.Show();
        }

        private void btnAddZipModel_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".zip"; // Default file extension
            dlg.Filter = "Zip files (.zip)|*.zip"; // Filter files by extension

            // Show open file dialog box
            bool? result = dlg.ShowDialog();

            if (result == true)
            {
                ((ModelManager)this.DataContext).ExtractModel(new FileInfo(dlg.FileName));
                ((ModelManager)this.DataContext).GetLocalModels();
            }
        }

        private void btnDeleteModel_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show("Are you sure?", "Delete Confirmation", System.Windows.MessageBoxButton.YesNo);
            if (messageBoxResult == MessageBoxResult.Yes)
            {
                var selectedModel = (MTModel)this.LocalModelList.SelectedItem;
                ((ModelManager)this.DataContext).UninstallModel(selectedModel);
            }
        }

        private void LbTodoList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Some buttons are dependent on the choice of model
        }

        
        

        private void btnContinueCustomization_Click(object sender, RoutedEventArgs e)
        {
            var selectedModel = (MTModel)this.LocalModelList.SelectedItem;
            selectedModel.ResumeTraining();
        }


        private void btnCustomizationProgress_Click(object sender, RoutedEventArgs e)
        {
            var selectedModel = (MTModel)this.LocalModelList.SelectedItem;
            CustomizationProgressWindow customizationProgressWindow = new CustomizationProgressWindow(selectedModel);
            customizationProgressWindow.Show();
        }

        private void btnTranslateWithModel_Click(object sender, RoutedEventArgs e)
        {
            var selectedModel = (MTModel)this.LocalModelList.SelectedItem;
            TranslateWindow translateWindow = new TranslateWindow(selectedModel);
            translateWindow.Show();
        }

        //TODO: Open a window where the test file is translated with the model.
        //Show the BLEU score and translation time etc.
        private void btnTestModel_Click(object sender, RoutedEventArgs e)
        {
            var selectedModel = (MTModel)this.LocalModelList.SelectedItem;
            TestWindow translateWindow = new TestWindow(selectedModel);
            translateWindow.Show();
        }

        private void btnEditModelTags_Click(object sender, RoutedEventArgs e)
        {
            var selectedModel = (MTModel)this.LocalModelList.SelectedItem;
            TagEditWindow tagEditWindow = new TagEditWindow(selectedModel);
            tagEditWindow.Show();
        }

        private void btnCustomizeModel_Click(object sender, RoutedEventArgs e)
        {
            var selectedModel = (MTModel)this.LocalModelList.SelectedItem;
            ModelCustomizerWindow customizeModel = new ModelCustomizerWindow(selectedModel);
            customizeModel.DataContext = this.DataContext;
            customizeModel.Show();
        }
    }

}
