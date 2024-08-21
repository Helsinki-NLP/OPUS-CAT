using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace OpusCatMtEngine
{
    public partial class EditPostEditRuleCollectionWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }


        private AutoEditRuleCollection _ruleCollection;

        public AutoEditRuleCollection RuleCollection
        {
            get
            {
                return _ruleCollection;
            }
            set
            {
                _ruleCollection = value;
                NotifyPropertyChanged();
            }
        }

        public EditPostEditRuleCollectionWindow() { }

        public EditPostEditRuleCollectionWindow(AutoEditRuleCollection selectedCollection)
        {

            this.RuleCollection = selectedCollection;
            InitializeComponent();
            this.Title = String.Format(Properties.Resources.EditRules_EditRuleCollectionTitle, selectedCollection.CollectionName);
            this.WindowHeader.Content = String.Format(Properties.Resources.EditRules_EditRuleCollectionTitle, selectedCollection.CollectionName);
            this.Tester.Refresh();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close(true);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close(false);
        }

        private async void CreateRule_Click(object sender, RoutedEventArgs e)
        {
            ICreateRuleWindow createRuleWindow = null;
            switch (this.RuleCollection.CollectionType)
            {
                case "postedit":
                    createRuleWindow = new CreatePostEditRuleWindow();
                    break;
                case "preedit":
                    createRuleWindow = new CreatePreEditRuleWindow();
                    break;
                default:
                    break;

            };

            if (createRuleWindow != null)
            {
                if (Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                {
                    var dialogResult = ((Window)createRuleWindow).ShowDialog<bool>(desktop.MainWindow);
                    await dialogResult;

                    if (dialogResult.Result)
                    {
                        this.RuleCollection.AddRule(createRuleWindow.CreatedRule);
                        this.Tester.Refresh();
                    }
                }
            }
        }

        private async void EditRule_Click(object sender, RoutedEventArgs e)
        {
            var rule = (AutoEditRule)this.AutoEditRuleCollectionList.SelectedItem;
            ICreateRuleWindow createRuleWindow = null;
            switch (this.RuleCollection.CollectionType)
            {
                case "postedit":
                    createRuleWindow = new CreatePostEditRuleWindow(rule);
                    break;
                case "preedit":
                    createRuleWindow = new CreatePreEditRuleWindow(rule);
                    break;
                default:
                    break;

            };

            if (createRuleWindow != null)
            {
                if (Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                {
                    var dialogResult = ((Window)createRuleWindow).ShowDialog<bool>(desktop.MainWindow);
                    await dialogResult;

                    if (dialogResult.Result)
                    {
                        this.RuleCollection.ReplaceRule(rule, createRuleWindow.CreatedRule);
                        this.Tester.Refresh();
                    }
                }
            }
        }

        private void DeleteRule_Click(object sender, RoutedEventArgs e)
        {
            var selectedRule = (AutoEditRule)this.AutoEditRuleCollectionList.SelectedItem;
            this.RuleCollection.EditRules.Remove(selectedRule);
            this.Tester.Refresh();
        }

    }
}
