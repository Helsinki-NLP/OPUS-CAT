using System;
using System.Collections.Generic;
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
    public partial class CreatePreEditRuleWindow : Window, ICreateRuleWindow
    {
        
        public AutoEditRule CreatedRule { get; set; }
        

        public CreatePreEditRuleWindow()
        {
            InitializeComponent();
        }

        public CreatePreEditRuleWindow(AutoEditRule rule)
        {
            InitializeComponent();
            this.PreEditPattern.Text = rule.SourcePattern;
            this.PreEditReplacement.Text = rule.Replacement;
            this.RuleDescription.Text = rule.Description;
            this.UseRegexInSourcePattern.IsChecked = rule.OutputPatternIsRegex;
        }

        

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            this.CreatedRule =
                new AutoEditRule()
                {
                    SourcePattern = this.PreEditPattern.Text,
                    SourcePatternIsRegex = this.UseRegexInSourcePattern.IsChecked.Value,
                    Replacement = this.PreEditReplacement.Text,
                    Description = this.RuleDescription.Text
                };

            //Validate regex
            try
            {
                var sourcePatternRegex = this.CreatedRule.SourcePatternRegex;
                this.DialogResult = true;
                this.Close();
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show($"Error in regular expression: {ex.Message}");
            }


            
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        
    }
}
