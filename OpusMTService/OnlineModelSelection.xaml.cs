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
    public partial class OnlineModelSelection: Window
    {
        private string sourceFilter = "";
        private string targetFilter = "";
        private string nameFilter = "";

        public OnlineModelSelection()
        {
            InitializeComponent();
            this.DataContextChanged += dataContextChanged;
        }

        private void dataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            ((ModelManager)this.DataContext).FilterOnlineModels(this.sourceFilter, this.targetFilter, this.nameFilter);
        }
        
        private void btnInstall_Click(object sender, RoutedEventArgs e)
        {
        }
        
        private void LbTodoList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void nameFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.nameFilter = ((TextBox)sender).Text;
            ((ModelManager)this.DataContext).FilterOnlineModels(this.sourceFilter, this.targetFilter, this.nameFilter);
        }

        private void sourceLangFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.sourceFilter = ((TextBox)sender).Text;
            ((ModelManager)this.DataContext).FilterOnlineModels(this.sourceFilter,this.targetFilter,this.nameFilter);
        }

        private void targetLangFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.targetFilter = ((TextBox)sender).Text;
            ((ModelManager)this.DataContext).FilterOnlineModels(this.sourceFilter, this.targetFilter, this.nameFilter);
        }
    }

}
