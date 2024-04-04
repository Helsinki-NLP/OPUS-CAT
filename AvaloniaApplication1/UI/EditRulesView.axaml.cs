using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Microsoft.Extensions.Hosting.Internal;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace OpusCatMtEngine
{
    public partial class EditRulesView : UserControl, INotifyPropertyChanged
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

        public EditRulesView() { }
        public EditRulesView(
            MTModel selectedModel,
            ObservableCollection<AutoEditRuleCollection> autoPreEditRuleCollections,
            ObservableCollection<AutoEditRuleCollection> autoPostEditRuleCollections)
        {
            this.Model = selectedModel;
            this.Title = String.Format(Properties.Resources.EditRules_EditRulesTitle, Model.Name);
            this.AutoPreEditRuleCollections = autoPreEditRuleCollections;
            this.AutoPostEditRuleCollections = autoPostEditRuleCollections;

            InitializeComponent();

            InitializeTester();
            this.AutoPreEditRuleCollectionList.ItemsSource = this.Model.AutoPreEditRuleCollections;
            this.AutoPostEditRuleCollectionList.ItemsSource = this.Model.AutoPostEditRuleCollections;
        }

        //Add testing controls for each collection
        private void InitializeTester()
        {

            this.RuleTester.Children.Clear();
            this.PreEditTesters = new List<TestPreEditRuleControl>();
            this.PostEditTesters = new List<TestPostEditRuleControl>();

            var inputBoxLabel = "Input to rule collection:";
            var inputOrigin = "Source text";

            foreach (var preEditRuleCollection in this.Model.AutoPreEditRuleCollections)
            {

                var title = $"Pre-edit rule collection";
                var testControl =
                    new TestPreEditRuleControl()
                    {
                        RuleCollection = preEditRuleCollection,
                        Title = title,
                        InputBoxLabel = inputBoxLabel,
                        InputOrigin = inputOrigin,
                        TestButtonVisibility = false,
                        ButtonText = "Test all pre- and postediting rules"
                    };

                inputOrigin = $"Output from {preEditRuleCollection.CollectionName}";
                this.PreEditTesters.Add(testControl);
                this.RuleTester.Children.Add(testControl);
            }


            inputBoxLabel = "Input to rule collection:";
            inputOrigin = "MT output";
            foreach (var postEditRuleCollection in this.Model.AutoPostEditRuleCollections)
            {
                var title = $"Post-edit rule collection";
                var testControl =
                    new TestPostEditRuleControl()
                    {
                        RuleCollection = postEditRuleCollection,
                        Title = title,
                        InputBoxLabel = inputBoxLabel,
                        InputOrigin = inputOrigin,
                        TestButtonVisibility = false,
                        SourceBoxVisibility = false
                    };
                inputOrigin = $"Output from {postEditRuleCollection.CollectionName}";

                this.PostEditTesters.Add(testControl);
                this.RuleTester.Children.Add(testControl);
            }
        }

        public MTModel Model { get => model; set => model = value; }
        public string Title { get; private set; }
        public ObservableCollection<AutoEditRuleCollection> AutoPreEditRuleCollections { get; private set; }
        public ObservableCollection<AutoEditRuleCollection> AutoPostEditRuleCollections { get; private set; }
        public List<TestPreEditRuleControl> PreEditTesters { get; private set; }
        public List<TestPostEditRuleControl> PostEditTesters { get; private set; }

        private async void CreatePreRule_Click(object sender, RoutedEventArgs e)
        {
            var createRuleWindow = new CreatePreEditRuleWindow();
            if (Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var dialogResult = createRuleWindow.ShowDialog<bool>(desktop.MainWindow);
                await dialogResult;

                if (dialogResult.Result)
                {
                    var newRuleCollection = new AutoEditRuleCollection()
                    {
                        CollectionName = createRuleWindow.CreatedRule.Description,
                        CollectionGuid = Guid.NewGuid().ToString(),
                        CollectionType = "preedit"
                    };
                    newRuleCollection.AddRule(createRuleWindow.CreatedRule);
                    newRuleCollection.Save();
                    this.Model.ModelConfig.AutoPreEditRuleCollectionGuids.Add(newRuleCollection.CollectionGuid);
                    this.Model.SaveModelConfig();
                    this.AutoPreEditRuleCollections.Add(newRuleCollection);
                    this.Model.AutoPreEditRuleCollections.Add(newRuleCollection);
                    InitializeTester();
                }
            }
        }



        private async void AddPreRuleCollection_Click(object sender, RoutedEventArgs e)
        {
            var addCollectionWindow =
                new AddEditRuleCollectionWindow(
                    this.AutoPreEditRuleCollections,
                    this.Model.AutoPreEditRuleCollections);

            if (Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var dialogResult = addCollectionWindow.ShowDialog<bool>(desktop.MainWindow);
                await dialogResult;

                if (dialogResult.Result)
                {
                    this.AddRuleCollection(
                        addCollectionWindow.RuleCollectionCheckBoxList,
                        this.AutoPreEditRuleCollections,
                        this.Model.AutoPreEditRuleCollections,
                        this.Model.ModelConfig.AutoPreEditRuleCollectionGuids);
                }
            }
        }

        private void AddRuleCollection(
            ObservableCollection<CheckBoxListItem<AutoEditRuleCollection>> ruleCollectionCheckBoxList,
            ObservableCollection<AutoEditRuleCollection> allCollections,
            ObservableCollection<AutoEditRuleCollection> modelCollections,
            ObservableCollection<string> modelGuids)
        {
            foreach (var collection in ruleCollectionCheckBoxList)
            {
                if (!allCollections.Contains(collection.Item))
                {
                    allCollections.Add(collection.Item);
                }

                if (collection.Checked)
                {
                    //Don't read the collection if it was already selected for the model
                    if (!modelCollections.Any(x => x.CollectionGuid == collection.Item.CollectionGuid))
                    {
                        modelCollections.Add(collection.Item);
                        modelGuids.Add(collection.Item.CollectionGuid);
                    }
                }
                else
                {
                    //If a collection was selected for the model and is now unchecked, remove it
                    var collectionPresent = modelCollections.SingleOrDefault(x => x.CollectionGuid == collection.Item.CollectionGuid);
                    if (collectionPresent != null)
                    {
                        modelCollections.Remove(collectionPresent);
                        modelGuids.Remove(collectionPresent.CollectionGuid);
                    }
                }
            }
            this.Model.SaveModelConfig();
            InitializeTester(); ;
        }

        private async void EditPreRuleCollection_Click(object sender, RoutedEventArgs e)
        {
            var selectedCollection = (AutoEditRuleCollection)this.AutoPreEditRuleCollectionList.SelectedItem;

            //Edit a clone of the collection, so that the changes can be canceled in the edit window
            var editCollectionWindow = new EditPreEditRuleCollectionWindow(selectedCollection.Clone());
            if (Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var dialogResult = editCollectionWindow.ShowDialog<bool>(desktop.MainWindow);
                await dialogResult;

                if (dialogResult.Result)
                {
                    selectedCollection.CopyValuesFromOtherCollection(editCollectionWindow.RuleCollection);
                    selectedCollection.Save();
                    //TODO: Check if list refreshes
                    //this.AutoPreEditRuleCollectionList.Items.Refresh();
                    InitializeTester();
                }
            }
            
            
        }

        private void RemoveRuleCollection(
            List<AutoEditRuleCollection> selectedCollections,
            ObservableCollection<string> guidList,
            ObservableCollection<AutoEditRuleCollection> collectionList)
        {
            foreach (var selectedCollection in selectedCollections)
            {
                guidList.Remove(selectedCollection.CollectionGuid);
                collectionList.Remove(selectedCollection);
            }
            this.Model.SaveModelConfig();
            InitializeTester();
        }

        private void RemovePreRuleCollection_Click(object sender, RoutedEventArgs e)
        {
            var selectedCollections =
                this.AutoPreEditRuleCollectionList.SelectedItems.Cast<AutoEditRuleCollection>().ToList();
            this.RemoveRuleCollection(
                selectedCollections,
                this.Model.ModelConfig.AutoPreEditRuleCollectionGuids,
                this.Model.AutoPreEditRuleCollections);
        }

        private async void DeletePreRuleCollection_Click(object sender, RoutedEventArgs e)
        {
            var selectedCollection = (AutoEditRuleCollection)this.AutoPreEditRuleCollectionList.SelectedItem;
            var messageBoxResultOk = await selectedCollection.Delete();
            if (messageBoxResultOk)
            {
                this.Model.AutoPreEditRuleCollections.Remove(selectedCollection);
                this.AutoPreEditRuleCollections.Remove(selectedCollection);
                this.Model.ModelConfig.AutoPreEditRuleCollectionGuids.Remove(selectedCollection.CollectionGuid);
                this.Model.SaveModelConfig();
                InitializeTester();
            }
        }

        private async void CreatePostRule_Click(object sender, RoutedEventArgs e)
        {
            var createRuleWindow = new CreatePostEditRuleWindow();
            if (Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var dialogResult = createRuleWindow.ShowDialog<bool>(desktop.MainWindow);
                await dialogResult;

                if (dialogResult.Result)
                {
                    var newRuleCollection = new AutoEditRuleCollection()
                    {
                        CollectionName = createRuleWindow.CreatedRule.Description,
                        CollectionGuid = Guid.NewGuid().ToString(),
                        CollectionType = "postedit",
                        GlobalCollection = false
                    };

                    newRuleCollection.AddRule(createRuleWindow.CreatedRule);
                    newRuleCollection.Save();
                    this.Model.ModelConfig.AutoPostEditRuleCollectionGuids.Add(newRuleCollection.CollectionGuid);
                    this.Model.SaveModelConfig();
                    this.AutoPostEditRuleCollections.Add(newRuleCollection);
                    this.Model.AutoPostEditRuleCollections.Add(newRuleCollection);
                    InitializeTester();
                }
            }
        }

        private async void AddPostRuleCollection_Click(object sender, RoutedEventArgs e)
        {
            var addCollectionWindow =
                new AddEditRuleCollectionWindow(
                    AutoPostEditRuleCollections,
                    this.Model.AutoPostEditRuleCollections);

            if (Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var dialogResult = addCollectionWindow.ShowDialog<bool>(desktop.MainWindow);
                await dialogResult;

                if (dialogResult.Result)
                {

                    this.AddRuleCollection(
                        addCollectionWindow.RuleCollectionCheckBoxList,
                        this.AutoPostEditRuleCollections,
                        this.Model.AutoPostEditRuleCollections,
                        this.Model.ModelConfig.AutoPostEditRuleCollectionGuids);
                }
            }
        }

        private async void EditPostRuleCollection_Click(object sender, RoutedEventArgs e)
        {
            var selectedCollection = (AutoEditRuleCollection)this.AutoPostEditRuleCollectionList.SelectedItem;

            //Edit a clone of the collection, so that the changes can be canceled in the edit window
            var editCollectionWindow = new EditPostEditRuleCollectionWindow(selectedCollection.Clone());
            if (Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var dialogResult = editCollectionWindow.ShowDialog<bool>(desktop.MainWindow);
                await dialogResult;

                if (dialogResult.Result)
                {
                    selectedCollection.CopyValuesFromOtherCollection(editCollectionWindow.RuleCollection);
                    selectedCollection.Save();
                    //TODO: check if this is needed
                    //this.AutoPostEditRuleCollectionList.ItemsSource.Refresh();
                    InitializeTester();
                }
            }

        }

        private void RemovePostRuleCollection_Click(object sender, RoutedEventArgs e)
        {
            var selectedCollections =
                this.AutoPostEditRuleCollectionList.SelectedItems.Cast<AutoEditRuleCollection>().ToList();
            this.RemoveRuleCollection(
                    selectedCollections,
                    this.Model.ModelConfig.AutoPostEditRuleCollectionGuids,
                    this.Model.AutoPostEditRuleCollections);
        }

        private void DeletePostRuleCollection_Click(object sender, RoutedEventArgs e)
        {
            var selectedCollection = (AutoEditRuleCollection)this.AutoPostEditRuleCollectionList.SelectedItem;
            selectedCollection.Delete();
            this.Model.AutoPostEditRuleCollections.Remove(selectedCollection);
            this.AutoPostEditRuleCollections.Remove(selectedCollection);
            this.Model.ModelConfig.AutoPostEditRuleCollectionGuids.Remove(selectedCollection.CollectionGuid);
            this.Model.SaveModelConfig();
            InitializeTester();
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
                    if (String.IsNullOrEmpty(tester.SourceText))
                    {
                        return;
                    }
                    else
                    {
                        rawSource = tester.SourceText;
                    }
                }
                tester.ProcessRules();
                previousTesterOutput = tester.OutputText;
            }

            //previousTesterOutput will now contain the pre-edited source for machine translation,
            //change it to MT output.
            //Do not apply edit rules here, since they will be visually applied in the tester
            var mtResult = this.Model.Translate(
                previousTesterOutput,
                this.Model.SourceLanguages.First(),
                this.Model.TargetLanguages.First(),
                applyEditRules: false).Result;

            previousTesterOutput = mtResult.Translation;

            foreach (var tester in this.PostEditTesters)
            {
                tester.SourceText = rawSource;
                tester.OutputText = previousTesterOutput;

                tester.ProcessRules();
                previousTesterOutput = tester.EditedOutputText;
            }

        }


        private void MoveCollectionDown(
            DataGrid collectionList,
            ObservableCollection<AutoEditRuleCollection> ruleCollection,
            ObservableCollection<string> collectionGuids
            )
        {
            var selectedItem = (AutoEditRuleCollection)collectionList.SelectedItem;

            var selectedItemIndex = ruleCollection.IndexOf(selectedItem);
            var guidIndex = collectionGuids.IndexOf(selectedItem.CollectionGuid);
            if (selectedItemIndex < ruleCollection.Count - 1)
            {
                ruleCollection.Move(selectedItemIndex, selectedItemIndex + 1);
                collectionGuids.Move(guidIndex, guidIndex + 1);
            }
            this.InitializeTester();
            this.Model.SaveModelConfig();
        }

        private void MovePreRuleCollectionDown_Click(object sender, RoutedEventArgs e)
        {
            this.MoveCollectionDown(
                this.AutoPreEditRuleCollectionList,
                this.Model.AutoPreEditRuleCollections,
                this.Model.ModelConfig.AutoPreEditRuleCollectionGuids);
        }

        private void MovePostRuleCollectionDown_Click(object sender, RoutedEventArgs e)
        {
            this.MoveCollectionDown(
                this.AutoPostEditRuleCollectionList,
                this.Model.AutoPostEditRuleCollections,
                this.Model.ModelConfig.AutoPostEditRuleCollectionGuids);
        }

        private void MoveCollectionUp(
            DataGrid collectionList,
            ObservableCollection<AutoEditRuleCollection> ruleCollection,
            ObservableCollection<string> collectionGuids)
        {
            var selectedItem = (AutoEditRuleCollection)collectionList.SelectedItem;
            var selectedItemIndex = ruleCollection.IndexOf(selectedItem);
            var guidIndex = collectionGuids.IndexOf(selectedItem.CollectionGuid);
            if (selectedItemIndex > 0)
            {
                ruleCollection.Move(selectedItemIndex, selectedItemIndex - 1);
                collectionGuids.Move(guidIndex, guidIndex - 1);
            }
            this.InitializeTester();
            this.Model.SaveModelConfig();
        }

        private void MovePreRuleCollectionUp_Click(object sender, RoutedEventArgs e)
        {
            this.MoveCollectionUp(
                this.AutoPreEditRuleCollectionList,
                this.Model.AutoPreEditRuleCollections,
                this.Model.ModelConfig.AutoPreEditRuleCollectionGuids);
        }

        private void MovePostRuleCollectionUp_Click(object sender, RoutedEventArgs e)
        {
            this.MoveCollectionUp(
                this.AutoPostEditRuleCollectionList,
                this.Model.AutoPostEditRuleCollections,
                this.Model.ModelConfig.AutoPostEditRuleCollectionGuids);
        }

        private void Tester_Expanded(object sender, RoutedEventArgs e)
        {
            //Scroll to tester when it is expanded
            var tester = (Expander)sender;
            tester.BringIntoView();
        }

    }
}
