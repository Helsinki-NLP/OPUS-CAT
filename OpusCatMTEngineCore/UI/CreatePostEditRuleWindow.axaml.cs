using Avalonia.Controls;
using Avalonia.Interactivity;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

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


        private async void SaveButton_Click(object sender, RoutedEventArgs e)
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
                this.Close(true);
            }
            catch (ArgumentException ex)
            {
                var box = MessageBoxManager.GetMessageBoxStandard(
                                $"Error in regular expression: {ex.Message}",
                                Properties.Resources.Finetune_NotEnoughSegmentsInTmx,
                                ButtonEnum.Ok);
                await box.ShowAsync();
                return;
            }



        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close(false);
        }

    }
}
