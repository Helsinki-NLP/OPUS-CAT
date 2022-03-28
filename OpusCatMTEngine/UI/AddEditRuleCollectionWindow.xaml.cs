using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
            foreach (var collection in allAutoEditRuleCollections)
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
