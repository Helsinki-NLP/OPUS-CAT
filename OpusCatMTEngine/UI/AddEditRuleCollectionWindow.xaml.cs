using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace OpusCatMTEngine
{
    
    /// <summary>
    /// Interaction logic for AddEditRuleCollectionWindow.xaml
    /// </summary>
    public partial class AddEditRuleCollectionWindow : Window
    {
        public ObservableCollection<CheckBoxListItem<AutoEditRuleCollection>> RuleCollectionCheckBoxList { get; set; }
        public IEnumerable<AutoEditRuleCollection> EditedRuleCollectionList { get; private set; }

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
        }

        private void DeleteTag_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.EditedRuleCollectionList = this.RuleCollectionCheckBoxList.Where(x => x.Checked).Select(y => y.Item);
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void ExportRules_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new CommonOpenFileDialog())
            {
                dialog.IsFolderPicker = true;
                CommonFileDialogResult result = dialog.ShowDialog();
                if (result == CommonFileDialogResult.Ok)
                {
                    try
                    {
                        var targetDir = dialog.FileName;
                        foreach (var checkedRule in RuleCollectionCheckBoxList.Where(x => x.Checked))
                        {
                            checkedRule.Item.Save(new DirectoryInfo(targetDir));
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Windows.Forms.MessageBox.Show($"Exception while exporting rules: {ex.Message}","Export error", MessageBoxButtons.OK);
                    }
                }
            }

            //Bring the rule addition window back to front
            this.Activate();
        }

        private void ImportRules_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new CommonOpenFileDialog())
            {
                dialog.Multiselect = true;
                dialog.Filters.Add(new CommonFileDialogFilter(".yml files", ".yml"));
                CommonFileDialogResult result = dialog.ShowDialog();
                List<AutoEditRuleCollection> collectionsToImport = new List<AutoEditRuleCollection>();
                var replaceExistingRules = !this.ReplaceCheckBox.IsChecked.Value;
                if (result == CommonFileDialogResult.Ok)
                {
                    try
                    {
                        foreach (var ymlFile in dialog.FileNames)
                        {
                            var collection =
                                AutoEditRuleCollection.CreateFromFile(
                                    new FileInfo(ymlFile),
                                    replaceExistingRules);
                            collection.GlobalCollection = true;
                            collectionsToImport.Add(collection);
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Windows.Forms.MessageBox.Show($"Exception while importing rules, no rules have been imported: {ex.Message}", "Export error", MessageBoxButtons.OK);
                    }
                    foreach (var collection in collectionsToImport)
                    {
                        collection.Save();
                        var checkboxListItem = new CheckBoxListItem<AutoEditRuleCollection>(collection);
                        this.RuleCollectionCheckBoxList.Add(checkboxListItem);
                        checkboxListItem.Checked = true;
                    }
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
