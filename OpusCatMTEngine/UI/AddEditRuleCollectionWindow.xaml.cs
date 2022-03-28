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
        public AddEditRuleCollectionWindow(ObservableCollection<AutoEditRuleCollection> autoPreEditRuleCollections)
        {
            InitializeComponent();
            this.AutoEditRuleCollectionList.ItemsSource = autoPreEditRuleCollections;
        }

        private void DeleteTag_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
