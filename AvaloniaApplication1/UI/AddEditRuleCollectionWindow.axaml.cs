using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Microsoft.Extensions.Logging;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace OpusCatMtEngine
{
    public partial class AddEditRuleCollectionWindow : Window
    {
        public ObservableCollection<CheckBoxListItem<AutoEditRuleCollection>> RuleCollectionCheckBoxList { get; set; }
        public IEnumerable<AutoEditRuleCollection> EditedRuleCollectionList { get; private set; }

        public AddEditRuleCollectionWindow() { }

        public AddEditRuleCollectionWindow(
            ObservableCollection<AutoEditRuleCollection> allAutoEditRuleCollections,
            ObservableCollection<AutoEditRuleCollection> modelAutoEditRuleCollections
            )
        {
            this.RuleCollectionCheckBoxList = new ObservableCollection<CheckBoxListItem<AutoEditRuleCollection>>();


            //Initialize checkbox values
            foreach (var collection in allAutoEditRuleCollections.Where(
                x => x.GlobalCollection || modelAutoEditRuleCollections.Contains(x)))
            {
                var checkboxListItem = new CheckBoxListItem<AutoEditRuleCollection>(collection);
                this.RuleCollectionCheckBoxList.Add(checkboxListItem);
                if (modelAutoEditRuleCollections.Contains(collection))
                {
                    checkboxListItem.Checked = true;
                }
            }

            InitializeComponent();

            this.AutoEditRuleCollectionList.ItemsSource = this.RuleCollectionCheckBoxList;
        }

        private void DeleteTag_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            this.EditedRuleCollectionList = this.RuleCollectionCheckBoxList.Where(x => x.Checked).Select(y => y.Item);
            this.Close(true);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close(false);
        }

        private async void ExportRules_Click(object sender, RoutedEventArgs e)
        {
            var topLevel = TopLevel.GetTopLevel(this);

            var dirs = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = "Select the folder where rule collection will be exported",
                AllowMultiple = false,
            });

            if (dirs.Count > 0) { 
                try
                {
                    var targetDir = dirs.First();
                    foreach (var checkedRule in RuleCollectionCheckBoxList.Where(x => x.Checked))
                    {
                        checkedRule.Item.Save(new DirectoryInfo(targetDir.TryGetLocalPath()));
                    }
                }
                catch (Exception ex)
                {
                    var box = MessageBoxManager.GetMessageBoxStandard(
                        "Export error",
                        $"Exception while exporting rules: {ex.Message}",
                        ButtonEnum.Ok);

                    var result = await box.ShowAsync();
                }
            }
            

            //Bring the rule addition window back to front
            this.Activate();
        }

        private async void ImportRules_Click(object sender, RoutedEventArgs e)
        {
            var topLevel = TopLevel.GetTopLevel(this);

            // Start async operation to open the dialog.
            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Select model zip file",
                AllowMultiple = true,
                FileTypeFilter = new[] { HelperFunctions.YmlFilePickerType}
            });

            List<AutoEditRuleCollection> collectionsToImport = new List<AutoEditRuleCollection>();
            var replaceExistingRules = !this.ReplaceCheckBox.IsChecked.Value;
            
            if (files.Count >= 1)
            {
                try
                {
                    foreach (var ymlFile in files)
                    {
                        var collection =
                            AutoEditRuleCollection.CreateFromFile(
                                new FileInfo(ymlFile.TryGetLocalPath()),
                                replaceExistingRules);
                        collection.GlobalCollection = true;
                        collectionsToImport.Add(collection);
                    }
                }
                catch (Exception ex)
                {
                    var box = MessageBoxManager.GetMessageBoxStandard(
                        "Import error",
                        $"Exception while importing rules, no rules have been imported: {ex.Message}",
                        ButtonEnum.Ok);

                    var result = await box.ShowAsync();
                    
                }

                foreach (var collection in collectionsToImport)
                {
                    collection.Save();
                    var checkboxListItem = new CheckBoxListItem<AutoEditRuleCollection>(collection);
                    this.RuleCollectionCheckBoxList.Add(checkboxListItem);
                    checkboxListItem.Checked = true;
                }
            
            }

            //Bring the rule addition window back to front
            this.Activate();
        }
        /*private void IncludeCollection_Checked(object sender, RoutedEventArgs e)
        {
            AutoEditRuleCollection collection = (AutoEditRuleCollection)(((CheckBox)sender).DataContext);
            this.RuleCollectionCheckBoxList.Add(collection);
        }

        private void IncludeCollection_Unchecked(object sender, RoutedEventArgs e)
        {
            AutoEditRuleCollection collection = (AutoEditRuleCollection)(((CheckBox)sender).DataContext);
            this.RuleCollectionCheckBoxList.Remove(collection);
        }*/
    }
}
