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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OpusMTService
{
    /// <summary>
    /// Interaction logic for ListBoxWithControls.xaml
    /// </summary>
    public partial class ListBoxWithControls : UserControl
    {

        ObservableCollection<string> modelList;

        public ListBoxWithControls()
        {
            InitializeComponent();
            this.DataContextChanged += dataContextChanged;
        }

        private void dataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            this.modelList = this.DataContext as ObservableCollection<string>;
        }
        
        
        private void btnAddItem_Click(object sender, RoutedEventArgs e)
        {
         
        }

        private void btnDeleteItem_Click(object sender, RoutedEventArgs e)
        {
         
        }

        private void btnEditItem_Click(object sender, RoutedEventArgs e)
        {
         
        }

        private void LbTodoList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }

}
