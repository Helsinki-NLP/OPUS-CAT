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
    public partial class CreatePostEditRuleWindow : Window, ICreateRuleWindow
    {
        private AutoEditRule rule;

        public AutoEditRule CreatedRule { get; set; }

        public CreatePostEditRuleWindow()
        {
            InitializeComponent();
        }

        public CreatePostEditRuleWindow(AutoEditRule rule)
        {
            InitializeComponent();
            this.SourcePattern.Text = rule.SourcePattern;
            this.PostEditReplacement.Text = rule.Replacement;
            this.PostEditPattern.Text = rule.OutputPattern;
            this.RuleDescription.Text = rule.Description;
        }
        

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            this.CreatedRule =
                new AutoEditRule()
                {
                    SourcePattern = this.SourcePattern.Text,
                    OutputPattern = this.PostEditPattern.Text,
                    Replacement = this.PostEditReplacement.Text,
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
