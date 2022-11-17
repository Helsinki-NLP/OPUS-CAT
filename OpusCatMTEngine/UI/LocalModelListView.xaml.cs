﻿using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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

namespace OpusCatMTEngine
{
    /// <summary>
    /// Interaction logic for ListBoxWithControls.xaml
    /// </summary>
    public partial class LocalModelListView : UserControl
    {
        private GridViewColumnHeader lastHeaderClicked;
        private ListSortDirection lastDirection;

        public LocalModelListView(ModelManager modelManager)
        {
            this.DataContext = modelManager;
            InitializeComponent();
        }
        
        private void btnOpenModelDir_Click(object sender, RoutedEventArgs e)
        {
            var selectedModel = (MTModel)this.LocalModelList.SelectedItem;
            Process.Start(selectedModel.InstallDir);
        }


        private void btnAddOnlineModel_Click(object sender, RoutedEventArgs e)
        {
            OnlineModelView onlineSelection = new OnlineModelView();
            onlineSelection.DataContext = this.DataContext;
            MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
            mainWindow.AddTab(new ActionTabItem() { Content = onlineSelection, Header = OpusCatMTEngine.Properties.Resources.Main_OnlineModelsTab, Closable = true });
            
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

        private void btnSetOverride_Click(object sender, RoutedEventArgs e)
        {
            var selectedModel = (MTModel)this.LocalModelList.SelectedItem;
            selectedModel.IsOverrideModel = true;
            selectedModel.IsOverridden = false;

            if (((ModelManager)this.DataContext).OverrideModel != null)
            {
                ((ModelManager)this.DataContext).OverrideModel.IsOverrideModel = false;
            }

            foreach (MTModel model in this.LocalModelList.Items)
            {
                if (model != selectedModel)
                {
                    model.IsOverridden = true;
                }
            }
            
            ((ModelManager)this.DataContext).OverrideModel = selectedModel;
            ((ModelManager)this.DataContext).OverrideModelTargetLanguage = selectedModel.TargetLanguages.First();
            ((ModelManager)this.DataContext).MoveOverrideToTop();
        }

        private void btnCancelOverride_Click(object sender, RoutedEventArgs e)
        {

            foreach (MTModel model in this.LocalModelList.Items)
            {
                model.IsOverridden = false;
            }

            ((ModelManager)this.DataContext).OverrideModel.IsOverrideModel = false;
            ((ModelManager)this.DataContext).OverrideModel = null;
        }

        private void btnDeleteModel_Click(object sender, RoutedEventArgs e)
        {
            var selectedModel = (MTModel)this.LocalModelList.SelectedItem;
            MessageBoxResult messageBoxResult = 
                System.Windows.MessageBox.Show(
                    String.Format(OpusCatMTEngine.Properties.Resources.Main_DeleteModelConfirmation,selectedModel.Name),
                    OpusCatMTEngine.Properties.Resources.Main_DeleteModelConfirmationTitle, System.Windows.MessageBoxButton.YesNo);
            if (messageBoxResult == MessageBoxResult.Yes)
            {
                
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

        private void btnpackageCustomModel_Click(object sender, RoutedEventArgs e)
        {
            var selectedModel = (MTModel)this.LocalModelList.SelectedItem;
            selectedModel.PackageModel();
        }

        private void btnCustomizationProgress_Click(object sender, RoutedEventArgs e)
        {
            var selectedModel = (MTModel)this.LocalModelList.SelectedItem;
            CustomizationProgressView customizationProgressView = new CustomizationProgressView(selectedModel);
            
            MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
            mainWindow.AddTab(new ActionTabItem() { Content = customizationProgressView, Header = customizationProgressView.Title, Closable = true });
        }

        private void btnTranslateWithModel_Click(object sender, RoutedEventArgs e)
        {
            var selectedModel = (MTModel)this.LocalModelList.SelectedItem;
            TranslateView translateView = new TranslateView(selectedModel);
            
            MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
            mainWindow.AddTab(new ActionTabItem() { Content = translateView, Header = translateView.Title, Closable = true });
        }
        

        private void btnEditModelTags_Click(object sender, RoutedEventArgs e)
        {
            var selectedModel = (MTModel)this.LocalModelList.SelectedItem;
            TagEditView tagEditView = new TagEditView(selectedModel);
        
            MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
            mainWindow.AddTab(new ActionTabItem() { Content = tagEditView, Header = tagEditView.Title, Closable = true });
        }

        private void btnCustomizeModel_Click(object sender, RoutedEventArgs e)
        {
            var selectedModel = (MTModel)this.LocalModelList.SelectedItem;
            ModelCustomizerView customizeModel = new ModelCustomizerView(selectedModel);
            customizeModel.DataContext = this.DataContext;

            MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
            mainWindow.AddTab(new ActionTabItem() { Content = customizeModel, Header = customizeModel.Title, Closable = true });
        }

        private void btnEvaluateModels_Click(object sender, RoutedEventArgs e)
        {
            IEnumerable<MTModel> selectedModels = this.LocalModelList.SelectedItems.OfType<MTModel>().ToList();
            ModelEvaluatorView evaluateModels = new ModelEvaluatorView(selectedModels);

            customizeModel.DataContext = this.DataContext;

            MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
            mainWindow.AddTab(new ActionTabItem() { Content = evaluateModels, Header = evaluateModels.Title, Closable = true });
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

                    ((ModelManager)this.DataContext).SortLocalModels(sortBy, direction);

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

        private void btnOpenModelInOverlay_Click(object sender, RoutedEventArgs e)
        {
            //TODO: open an overlay that is always on top and displays translation
        }

        private void EditRules_Click(object sender, RoutedEventArgs e)
        {
            var selectedModel = (MTModel)this.LocalModelList.SelectedItem;
            EditRulesView editRules = 
                new EditRulesView(
                    selectedModel, 
                    ((ModelManager)this.DataContext).AutoPreEditRuleCollections,
                    ((ModelManager)this.DataContext).AutoPostEditRuleCollections);
            editRules.DataContext = this.DataContext;

            MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
            mainWindow.AddTab(new ActionTabItem() { Content = editRules, Header = editRules.Title, Closable = true });
        }

        private void TermList_Click(object sender, RoutedEventArgs e)
        {
            var selectedModel = (MTModel)this.LocalModelList.SelectedItem;
            TerminologyView terminology =
                new TerminologyView(
                    selectedModel,
                    ((ModelManager)this.DataContext).Terminologies);
            terminology.DataContext = this.DataContext;

            MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
            mainWindow.AddTab(new ActionTabItem() { Content = terminology, Header = terminology.Title, Closable = true });
        }
    }

}
