using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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
    /// Interaction logic for EditEditRuleCollectionWindow.xaml
    /// </summary>
    public partial class EditEditRuleCollectionWindow : Window, INotifyPropertyChanged
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

        public EditEditRuleCollectionWindow(AutoEditRuleCollection selectedCollection)
        {
            this.RuleCollection = selectedCollection;
            InitializeComponent();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void CreateRule_Click(object sender, RoutedEventArgs e)
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
                var dialogResult = ((Window)createRuleWindow).ShowDialog();

                if (dialogResult != null && dialogResult.Value)
                {
                    this.RuleCollection.AddRule(createRuleWindow.CreatedRule);
                }
            }
        }

        private void EditRule_Click(object sender, RoutedEventArgs e)
        {
            var rule = (AutoEditRule)this.AutoEditRuleCollectionList.SelectedItem;
            ICreateRuleWindow createRuleWindow = null;
            switch (this.RuleCollection.CollectionType)
            {
                case "postedit":
                    break;
                case "preedit":
                    createRuleWindow = new CreatePreEditRuleWindow(rule);
                    break;
                default:
                    break;

            };

            if (createRuleWindow != null)
            {
                var dialogResult = ((Window)createRuleWindow).ShowDialog();

                if (dialogResult != null && dialogResult.Value)
                {
                    this.RuleCollection.ReplaceRule(rule, createRuleWindow.CreatedRule);
                }
            }
        }

        private void DeleteRule_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
