﻿using System;
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
using static OpusCatMtEngine.AutoEditRule;

namespace OpusCatMtEngine
{
    public partial class CreatePostEditRuleWindow : Window, ICreateRuleWindow, INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        
        
        public AutoEditRule CreatedRule { get; set; }
        

        public CreatePostEditRuleWindow()
        {
            this.DataContext = this;
            InitializeComponent();
        }

        public CreatePostEditRuleWindow(AutoEditRule rule)
        {
            InitializeComponent();
            
            if (!String.IsNullOrWhiteSpace(rule.SourcePattern))
            {
                this.SourcePatternCheckbox.IsChecked = true;
                this.SourcePattern.Text = rule.SourcePattern;
                this.UseRegexInSourcePattern.IsChecked = rule.SourcePatternIsRegex;
            }
            this.PostEditReplacement.Text = rule.Replacement;
            this.PostEditPattern.Text = rule.OutputPattern;
            this.UseRegexInPostEditPattern.IsChecked = rule.OutputPatternIsRegex;
            this.RuleDescription.Text = rule.Description;
        }


        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            this.CreatedRule =
                new AutoEditRule()
                {
                    SourcePattern = this.SourcePatternCheckbox.IsChecked.Value ? this.SourcePattern.Text : "",
                    SourcePatternIsRegex = this.UseRegexInSourcePattern.IsChecked.Value,
                    OutputPattern = this.PostEditPattern.Text,
                    OutputPatternIsRegex = this.UseRegexInPostEditPattern.IsChecked.Value,
                    Replacement = this.PostEditReplacement.Text,
                    Description = this.RuleDescription.Text
                };

            //Validate regex
            try
            {
                var sourcePatternRegex = this.CreatedRule.SourcePatternRegex;
                var outputPatternRegex = this.CreatedRule.OutputPatternRegex;
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
