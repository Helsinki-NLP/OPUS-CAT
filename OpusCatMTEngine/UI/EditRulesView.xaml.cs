using OpusMTInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
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
using System.Windows.Threading;

namespace OpusCatMTEngine
{
    /// <summary>
    /// Interaction logic for TranslateWindow.xaml
    /// </summary>
    public partial class EditRulesView: UserControl
    {

        private MTModel model;

        public EditRulesView(MTModel selectedModel)
        {
            this.Model = selectedModel;
            this.DataContext = selectedModel;
            this.Title = String.Format(OpusCatMTEngine.Properties.Resources.EditRules_EditRulesTitle,Model.Name);
            InitializeComponent();
            this.AutoPreEditRuleCollectionList.ItemsSource = selectedModel.ModelConfig.AutoPreEditRuleCollections;
            this.AutoPostEditRuleCollectionList.ItemsSource = selectedModel.ModelConfig.AutoPostEditRuleCollections;
        }

        public MTModel Model { get => model; set => model = value; }
        public string Title { get; private set; }

        private void CreatePreRule_Click(object sender, RoutedEventArgs e)
        {
            var createRuleWindow = new CreateEditRuleWindow();
            createRuleWindow.DataContext = ((ModelManager)this.DataContext).AutoPreEditRuleCollections;
            var dialogResult = createRuleWindow.ShowDialog();
        }

        private void AddPreRuleCollection_Click(object sender, RoutedEventArgs e)
        {
            var addCollectionWindow = new AddEditRuleCollectionWindow();
            addCollectionWindow.DataContext = ((ModelManager)this.DataContext).AutoPreEditRuleCollections;
            var dialogResult = addCollectionWindow.ShowDialog();
        }

        private void EditPreRuleCollection_Click(object sender, RoutedEventArgs e)
        {

        }

        private void RemovePreRuleCollection_Click(object sender, RoutedEventArgs e)
        {

        }

        private void DeletePreRuleCollection_Click(object sender, RoutedEventArgs e)
        {

        }

        private void CreatePostRule_Click(object sender, RoutedEventArgs e)
        {

        }

        private void AddPostRuleCollection_Click(object sender, RoutedEventArgs e)
        {

        }

        private void EditPostRuleCollection_Click(object sender, RoutedEventArgs e)
        {

        }

        private void RemovePostRuleCollection_Click(object sender, RoutedEventArgs e)
        {

        }

        private void DeletePostRuleCollection_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
