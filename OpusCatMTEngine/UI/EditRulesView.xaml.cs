using OpusMTInterface;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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
            InitializeTester();
            this.AutoPreEditRuleCollectionList.ItemsSource = this.ModelAutoPreEditRuleCollections;
            this.AutoPostEditRuleCollectionList.ItemsSource = this.ModelAutoPostEditRuleCollections;
            
        }

        //Add testing controls for each collection
        private void InitializeTester()
        {
            this.PreEditTesters = new List<TestPreEditRuleControl>();
            this.PostEditTesters = new List<TestPostEditRuleControl>();
            var inputBoxLabel = "Input to rule collection: Source text";
            
            foreach (var preEditRuleCollection in this.ModelAutoPreEditRuleCollections)
            {
                var title = $"Rule collection {preEditRuleCollection.CollectionName}";
                var testControl =
                    new TestPreEditRuleControl()
                    {
                        RuleCollection = preEditRuleCollection,
                        Title = title,
                        InputBoxLabel = inputBoxLabel,
                        TestButtonVisibility = Visibility.Collapsed,
                        ButtonText = "Test all pre- and postediting rules"
                    };

                inputBoxLabel = $"Input to rule collection: Output from {preEditRuleCollection.CollectionName}";
                this.PreEditTesters.Add(testControl);
                this.RuleTester.Children.Add(testControl);
            }


            inputBoxLabel = "Input to rule collection: MT output";
            foreach (var postEditRuleCollection in this.ModelAutoPostEditRuleCollections)
            {
                var title = $"Rule collection {postEditRuleCollection.CollectionName}";
                var testControl =
                    new TestPostEditRuleControl()
                    {
                        RuleCollection = postEditRuleCollection,
                        Title = title,
                        InputBoxLabel = inputBoxLabel,
                        TestButtonVisibility = Visibility.Collapsed
                    };
                inputBoxLabel = $"Input to rule collection: Output from {postEditRuleCollection.CollectionName}";

                this.PostEditTesters.Add(testControl);
                this.RuleTester.Children.Add(testControl);
            }
        }

        public MTModel Model { get => model; set => model = value; }
        public string Title { get; private set; }
        public ObservableCollection<AutoEditRuleCollection> AutoPreEditRuleCollections { get; private set; }
        public ObservableCollection<AutoEditRuleCollection> AutoPostEditRuleCollections { get; private set; }
        public ObservableCollection<AutoEditRuleCollection> ModelAutoPreEditRuleCollections { get; set; }
        public ObservableCollection<AutoEditRuleCollection> ModelAutoPostEditRuleCollections { get; set; }
        public List<TestPreEditRuleControl> PreEditTesters { get; private set; }
        public List<TestPostEditRuleControl> PostEditTesters { get; private set; }

        private void CreatePreRule_Click(object sender, RoutedEventArgs e)
        {
            var createRuleWindow = new CreatePreEditRuleWindow();
            
            var dialogResult = createRuleWindow.ShowDialog();

            if (dialogResult != null && dialogResult.Value)
            {
                var newRuleCollection = new AutoEditRuleCollection()
                {
                    CollectionName = "new collection",
                    CollectionGuid = Guid.NewGuid().ToString(),
                    CollectionType = "preedit"
                };
                newRuleCollection.AddRule(createRuleWindow.CreatedRule);
                newRuleCollection.Save();
                this.Model.ModelConfig.AutoPreEditRuleCollectionGuids.Add(newRuleCollection.CollectionGuid);
                this.Model.SaveModelConfig();
                this.AutoPreEditRuleCollections.Add(newRuleCollection);
                this.ModelAutoPreEditRuleCollections.Add(newRuleCollection);
            }
        }
        
        private void AddPreRuleCollection_Click(object sender, RoutedEventArgs e)
        {
            var addCollectionWindow = 
                new AddEditRuleCollectionWindow(
                    AutoPreEditRuleCollections, 
                    this.ModelAutoPreEditRuleCollections);
            var dialogResult = addCollectionWindow.ShowDialog();
            if (dialogResult.Value)
            {
                this.ModelAutoPreEditRuleCollections.Clear();
                foreach (var collection in addCollectionWindow.RuleCollectionCheckBoxList)
                {
                    if (collection.Checked)
                    {
                        this.ModelAutoPreEditRuleCollections.Add(collection.Item);
                        this.Model.ModelConfig.AutoPreEditRuleCollectionGuids.Add(collection.Item.CollectionGuid);
                    }
                }
                this.Model.SaveModelConfig();
            }
        }

        private void EditPreRuleCollection_Click(object sender, RoutedEventArgs e)
        {
            var selectedCollection = (AutoEditRuleCollection)this.AutoPreEditRuleCollectionList.SelectedItem;
            var editCollectionWindow = new EditPreEditRuleCollectionWindow(selectedCollection);
            var dialogResult = editCollectionWindow.ShowDialog();
            selectedCollection.Save();
        }

        private void RemovePreRuleCollection_Click(object sender, RoutedEventArgs e)
        {
            var selectedCollection = (AutoEditRuleCollection)this.AutoPreEditRuleCollectionList.SelectedItem;
            this.Model.ModelConfig.AutoPreEditRuleCollectionGuids.Remove(selectedCollection.CollectionGuid);
            this.ModelAutoPreEditRuleCollections.Remove(selectedCollection);
            this.Model.SaveModelConfig();
        }

        private void DeletePreRuleCollection_Click(object sender, RoutedEventArgs e)
        {
            var selectedCollection = (AutoEditRuleCollection)this.AutoPreEditRuleCollectionList.SelectedItem;
            selectedCollection.Delete();
            this.ModelAutoPreEditRuleCollections.Remove(selectedCollection);
            this.AutoPreEditRuleCollections.Remove(selectedCollection);
            this.Model.ModelConfig.AutoPreEditRuleCollectionGuids.Remove(selectedCollection.CollectionGuid);
            this.Model.SaveModelConfig();
        }

        private void CreatePostRule_Click(object sender, RoutedEventArgs e)
        {
            var createRuleWindow = new CreatePostEditRuleWindow();

            var dialogResult = createRuleWindow.ShowDialog();

            if (dialogResult != null && dialogResult.Value)
            {
                var newRuleCollection = new AutoEditRuleCollection()
                {
                    CollectionName = "new collection",
                    CollectionGuid = Guid.NewGuid().ToString(),
                    CollectionType = "postedit"
                };

                newRuleCollection.AddRule(createRuleWindow.CreatedRule);
                newRuleCollection.Save();
                this.Model.ModelConfig.AutoPostEditRuleCollectionGuids.Add(newRuleCollection.CollectionGuid);
                this.Model.SaveModelConfig();
                this.AutoPostEditRuleCollections.Add(newRuleCollection);
                this.ModelAutoPostEditRuleCollections.Add(newRuleCollection);
            }
        }

        private void AddPostRuleCollection_Click(object sender, RoutedEventArgs e)
        {
            var addCollectionWindow =
                new AddEditRuleCollectionWindow(
                    AutoPostEditRuleCollections,
                    this.ModelAutoPostEditRuleCollections);
            var dialogResult = addCollectionWindow.ShowDialog();
            if (dialogResult.Value)
            {
                this.ModelAutoPostEditRuleCollections.Clear();
                foreach (var collection in addCollectionWindow.RuleCollectionCheckBoxList)
                {
                    if (collection.Checked)
                    {
                        this.ModelAutoPostEditRuleCollections.Add(collection.Item);
                        this.Model.ModelConfig.AutoPostEditRuleCollectionGuids.Add(collection.Item.CollectionGuid);
                    }
                }
                this.Model.SaveModelConfig();
            }
        }

        private void EditPostRuleCollection_Click(object sender, RoutedEventArgs e)
        {
            var selectedCollection = (AutoEditRuleCollection)this.AutoPostEditRuleCollectionList.SelectedItem;
            var editCollectionWindow = new EditPostEditRuleCollectionWindow(selectedCollection);
            var dialogResult = editCollectionWindow.ShowDialog();
            selectedCollection.Save();
        }

        private void RemovePostRuleCollection_Click(object sender, RoutedEventArgs e)
        {
            var selectedCollection = (AutoEditRuleCollection)this.AutoPostEditRuleCollectionList.SelectedItem;
            this.Model.ModelConfig.AutoPostEditRuleCollectionGuids.Remove(selectedCollection.CollectionGuid);
            this.ModelAutoPostEditRuleCollections.Remove(selectedCollection);
            this.Model.SaveModelConfig();
        }

        private void DeletePostRuleCollection_Click(object sender, RoutedEventArgs e)
        {
            var selectedCollection = (AutoEditRuleCollection)this.AutoPostEditRuleCollectionList.SelectedItem;
            selectedCollection.Delete();
            this.ModelAutoPostEditRuleCollections.Remove(selectedCollection);
            this.AutoPostEditRuleCollections.Remove(selectedCollection);
            this.Model.ModelConfig.AutoPostEditRuleCollectionGuids.Remove(selectedCollection.CollectionGuid);
            this.Model.SaveModelConfig();
        }

        private void TestRules_Click(object sender, RoutedEventArgs e)
        {
            string previousTesterOutput = null;
            string rawSource = null;
            foreach (var tester in this.PreEditTesters)
            {
                if (previousTesterOutput != null)
                {
                    tester.SourceText = previousTesterOutput;
                }
                else
                {
                    rawSource = tester.SourceText;
                }
                tester.ProcessRules();
                previousTesterOutput = tester.OutputText;
            }

            //previousTesterOutput will now contain the pre-edited source for machine translation,
            //change it to MT output
            var mtResult = this.Model.Translate(
                previousTesterOutput,
                this.Model.SourceLanguages.First(),
                this.Model.SourceLanguages.First()).Result;

            previousTesterOutput = mtResult.Translation;

            foreach (var tester in this.PostEditTesters)
            {
                tester.SourceText = rawSource;
                tester.OutputText = previousTesterOutput;
                
                tester.ProcessRules();
                previousTesterOutput = tester.EditedOutputText;
            }

        }
    }
}
