using Avalonia.Controls;
using Avalonia.Interactivity;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System;
using Avalonia.Controls.ApplicationLifetimes;
using System.Linq;
using System.Collections.Generic;
using Avalonia.Platform.Storage;

namespace OpusCatMtEngine
{
    public partial class LocalModelListView : UserControl
    {
        //private System.Windows.Controls.GridViewColumnHeader lastHeaderClicked;
        private ListSortDirection lastDirection;

        public LocalModelListView()
        {
            InitializeComponent();
        }

        public LocalModelListView(ModelManager modelManager)
        {
            this.DataContext = modelManager;
            InitializeComponent();
            
        }

        private void btnOpenModelDir_Click(object sender, RoutedEventArgs e)
        {
            var selectedModel = (MTModel)this.LocalModelList.SelectedItem;
#if WINDOWS
            Process.Start("explorer.exe", selectedModel.InstallDir);
#elif LINUX
            Process.Start("nautilus", selectedModel.InstallDir); 
#endif
        }


        private void btnAddOnlineModel_Click(object sender, RoutedEventArgs e)
        {
            OnlineModelView onlineSelection = new OnlineModelView((ModelManager)this.DataContext);
            

            if (Avalonia.Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                MainWindow mainWindow = (MainWindow)desktop.MainWindow;
                mainWindow.AddTab(new ActionTabItem() { Content = onlineSelection, Header = OpusCatMtEngine.Properties.Resources.Main_OnlineModelsTab, Closable = true });
            }

        }

        private async void btnAddZipModel_Click(object sender, RoutedEventArgs e)
        {
            var topLevel = TopLevel.GetTopLevel(this);

            // Start async operation to open the dialog.
            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Select model zip file",
                AllowMultiple = false,
                FileTypeFilter = new[] { HelperFunctions.ZipFilePickerType }

            });

            if (files.Count >= 1)
            {
                //TODO: Fix the path handling here, avalonia adds file:/// to path
                var path = files.First().Path;
                ((ModelManager)this.DataContext).ExtractModel(new FileInfo(path.AbsolutePath));
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

            foreach (MTModel model in this.LocalModelList.ItemsSource)
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

            foreach (MTModel model in this.LocalModelList.ItemsSource)
            {
                model.IsOverridden = false;
            }

            ((ModelManager)this.DataContext).OverrideModel.IsOverrideModel = false;
            ((ModelManager)this.DataContext).OverrideModel = null;
        }

        private async void btnDeleteModel_Click(object sender, RoutedEventArgs e)
        {
            var selectedModel = (MTModel)this.LocalModelList.SelectedItem;

            string messageBoxText = String.Format(OpusCatMtEngine.Properties.Resources.Main_DeleteModelConfirmation, selectedModel.Name);
            var box = MessageBoxManager.GetMessageBoxStandard(
                OpusCatMtEngine.Properties.Resources.Main_DeleteModelConfirmationTitle,
                messageBoxText,
                ButtonEnum.YesNo);

            var messageBoxResult = await box.ShowAsync();
            
            
            if (messageBoxResult == ButtonResult.Yes)
            {

                ((ModelManager)this.DataContext).UninstallModel(selectedModel);
            }
        }

        private void LbTodoList_SelectionChanged(object sender, Avalonia.Controls.SelectionChangedEventArgs e)
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

            if (Avalonia.Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                MainWindow mainWindow = (MainWindow)desktop.MainWindow;
                mainWindow.AddTab(new ActionTabItem() { Content = customizationProgressView, Header = customizationProgressView.Title, Closable = true });
            }
        }

        private void btnTranslateWithModel_Click(object sender, RoutedEventArgs e)
        {
            var selectedModel = (MTModel)this.LocalModelList.SelectedItem;

            TranslateView translateView = new TranslateView(selectedModel);

            if (Avalonia.Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                MainWindow mainWindow = (MainWindow)desktop.MainWindow;
                mainWindow.AddTab(new ActionTabItem() { Content = translateView, Header = translateView.Title, Closable = true });
            }

        }


        private void btnEditModelTags_Click(object sender, RoutedEventArgs e)
        {
            var selectedModel = (MTModel)this.LocalModelList.SelectedItem;
            TagEditView tagEditView = new TagEditView(selectedModel);
            if (Avalonia.Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                MainWindow mainWindow = (MainWindow)desktop.MainWindow;
                mainWindow.AddTab(new ActionTabItem() { Content = tagEditView, Header = tagEditView.Title, Closable = true });
            }
        }

        private void btnCustomizeModel_Click(object sender, RoutedEventArgs e)
        {
            var selectedModel = (MTModel)this.LocalModelList.SelectedItem;
            ModelCustomizerView customizeModel = new ModelCustomizerView(selectedModel);
            customizeModel.DataContext = this.DataContext;

            if (Avalonia.Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                MainWindow mainWindow = (MainWindow)desktop.MainWindow;
                mainWindow.AddTab(new ActionTabItem() { Content = customizeModel, Header = customizeModel.Title, Closable = true });
            }
        }

        private void btnEvaluateModels_Click(object sender, RoutedEventArgs e)
        {
            IEnumerable<MTModel> selectedModels = this.LocalModelList.SelectedItems.OfType<MTModel>().ToList();
            ModelEvaluatorView evaluateModels = new ModelEvaluatorView(selectedModels);

            customizeModel.DataContext = this.DataContext;

            if (Avalonia.Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                MainWindow mainWindow = (MainWindow)desktop.MainWindow;
                mainWindow.AddTab(new ActionTabItem() { Content = evaluateModels, Header = evaluateModels.Title, Closable = true });
            }
        }

        /*TODO: this probably isn't needed with DataGrid
         * private void GridViewColumnHeaderClickedHandler(object sender, RoutedEventArgs e)
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
        }*/

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

            if (Avalonia.Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                MainWindow mainWindow = (MainWindow)desktop.MainWindow;
                mainWindow.AddTab(new ActionTabItem() { Content = editRules, Header = editRules.Title, Closable = true });
            }
        }

        private void TermList_Click(object sender, RoutedEventArgs e)
        {
            var selectedModel = (MTModel)this.LocalModelList.SelectedItem;
            TerminologyView terminology =
                new TerminologyView(
                    selectedModel,
                    ((ModelManager)this.DataContext).Terminologies);
            terminology.DataContext = this.DataContext;

            if (Avalonia.Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                MainWindow mainWindow = (MainWindow)desktop.MainWindow;
                mainWindow.AddTab(new ActionTabItem() { Content = terminology, Header = terminology.Title, Closable = true });
            }
        }

    }
}