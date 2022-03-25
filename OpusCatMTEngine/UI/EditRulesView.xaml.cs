using OpusMTInterface;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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
    public partial class EditRulesView: UserControl, INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private MTModel model;

        public EditRulesView(
            MTModel selectedModel,
            ObservableCollection<AutoEditRuleCollection> autoPreEditRuleCollections,
            ObservableCollection<AutoEditRuleCollection> autoPostEditRuleCollections)
        {
            this.Model = selectedModel;
            this.Title = String.Format(OpusCatMTEngine.Properties.Resources.EditRules_EditRulesTitle,Model.Name);
            
            this.AutoPreEditRuleCollections = autoPreEditRuleCollections;
            this.AutoPostEditRuleCollections = autoPostEditRuleCollections;
            this.ModelAutoPreEditRuleCollections = new ObservableCollection<AutoEditRuleCollection>(
                autoPreEditRuleCollections.Where(x => this.Model.ModelConfig.AutoPreEditRuleCollectionGuids.Contains(x.CollectionGuid)));
            this.ModelAutoPostEditRuleCollections = new ObservableCollection<AutoEditRuleCollection>(
                autoPostEditRuleCollections.Where(x => this.Model.ModelConfig.AutoPostEditRuleCollectionGuids.Contains(x.CollectionGuid)));
            InitializeComponent();
            this.AutoPreEditRuleCollectionList.ItemsSource = this.ModelAutoPreEditRuleCollections;
            this.AutoPostEditRuleCollectionList.ItemsSource = this.Model.ModelConfig.AutoPostEditRuleCollectionGuids;
            
        }

        public MTModel Model { get => model; set => model = value; }
        public string Title { get; private set; }
        public ObservableCollection<AutoEditRuleCollection> AutoPreEditRuleCollections { get; private set; }
        public ObservableCollection<AutoEditRuleCollection> AutoPostEditRuleCollections { get; private set; }
        public ObservableCollection<AutoEditRuleCollection> ModelAutoPreEditRuleCollections { get; set; }
        public ObservableCollection<AutoEditRuleCollection> ModelAutoPostEditRuleCollections { get; set; }

        private void CreatePreRule_Click(object sender, RoutedEventArgs e)
        {
            var createRuleWindow = new CreatePreEditRuleWindow();
            
            var dialogResult = createRuleWindow.ShowDialog();

            if (dialogResult != null && dialogResult.Value)
            {
                var newRuleCollection = new AutoEditRuleCollection()
                    { CollectionName = "new collection", CollectionGuid = Guid.NewGuid().ToString() };
                newRuleCollection.AddRule(createRuleWindow.CreatedRule);
                this.Model.ModelConfig.AutoPreEditRuleCollectionGuids.Add(newRuleCollection.CollectionGuid);
                this.AutoPreEditRuleCollections.Add(newRuleCollection);
                this.ModelAutoPreEditRuleCollections.Add(newRuleCollection);
            }
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
            var createRuleWindow = new CreatePostEditRuleWindow();

            var dialogResult = createRuleWindow.ShowDialog();

            if (dialogResult != null && dialogResult.Value)
            {
                var newRuleCollection = new AutoEditRuleCollection()
                { CollectionName = "new collection", CollectionGuid = Guid.NewGuid().ToString() };
                newRuleCollection.AddRule(createRuleWindow.CreatedRule);
                this.Model.ModelConfig.AutoPostEditRuleCollectionGuids.Add(newRuleCollection.CollectionGuid);
                this.AutoPostEditRuleCollections.Add(newRuleCollection);
                this.ModelAutoPostEditRuleCollections.Add(newRuleCollection);
            }
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
