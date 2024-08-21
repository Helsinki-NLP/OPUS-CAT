using Avalonia.Controls;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia;
using System;
using Avalonia.Interactivity;

namespace OpusCatMtEngine
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
            this.UseRegexInSourcePattern.IsChecked = rule.SourcePatternIsRegex;
        }



        private async void SaveButton_Click(object sender, RoutedEventArgs e)
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
                this.Close(true);
            }
            catch (ArgumentException ex)
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Invalid regular expression",
                                 $"Error in regular expression: {ex.Message}",
                                 ButtonEnum.Ok);
                await box.ShowAsync();
            }

        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close(false);
        }


    }
}
