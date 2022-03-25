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
    public partial class CreatePostEditRuleWindow : Window
    {
       

        public AutoEditRule CreatedRule { get; private set; }

        public CreatePostEditRuleWindow()
        {
            InitializeComponent();
        }

        private void PreEditTest_Click(object sender, RoutedEventArgs e)
        {
            var ruleCollection = new AutoEditRuleCollection();
            ruleCollection.AddRule(
                new AutoEditRule() {
                    SourcePattern = this.SourcePattern.Text,
                    OutputPattern = this.PostEditPattern.Text,
                    Replacement = this.PostEditReplacement.Text
                });


            TextRange sourceTextRange = new TextRange(this.SourceBox.Document.ContentStart, this.SourceBox.Document.ContentEnd);
            var sourceText = sourceTextRange.Text;

            TextRange outputTextRange = new TextRange(this.OutputBox.Document.ContentStart, this.OutputBox.Document.ContentEnd);
            var outputText = outputTextRange.Text;
            try
            {
                var result = ruleCollection.ProcessPostEditRules(sourceText, outputText);
                if (this.SourcePatternCheckbox.IsChecked.Value)
                {
                    this.PopulateSourceBox(result);
                }
                this.PopulateOutputBox(result);
                this.PopulateTargetBox(result);
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show($"Error in regular expression: {ex.Message}");
            }


        }

        private void PopulateSourceBox(AutoEditResult result)
        {
            //Store the source text, use it as basis of the source text with match highlights
            TextRange textRange = new TextRange(this.SourceBox.Document.ContentStart, this.SourceBox.Document.ContentEnd);
            var sourceText = textRange.Text;

            int nonMatchStartIndex = 0;
            Paragraph matchHighlightSource = new Paragraph();
            foreach (var replacement in result.AppliedReplacements)
            {

                if (nonMatchStartIndex < replacement.Match.Index)
                {
                    var nonMatchText = 
                        sourceText.Substring(
                            nonMatchStartIndex, replacement.Match.Index - nonMatchStartIndex);
                    matchHighlightSource.Inlines.Add(nonMatchText);
                }

                var matchText = replacement.Match.Value;
                var matchRun = new Run(matchText)
                    { Background = Brushes.Chartreuse, ToolTip = replacement.Rule.SourcePattern };

                matchHighlightSource.Inlines.Add(matchRun);
                
                nonMatchStartIndex = replacement.Match.Index + replacement.Match.Length;
            }

            if (nonMatchStartIndex < sourceText.Length)
            {
                var nonMatchText =
                        sourceText.Substring(
                            nonMatchStartIndex);
                matchHighlightSource.Inlines.Add(nonMatchText);
            }

            this.SourceBox.Document.Blocks.Clear();
            this.SourceBox.Document.Blocks.Add(matchHighlightSource);
        }

        private void PopulateOutputBox(AutoEditResult result)
        {
            //Store the source text, use it as basis of the source text with match highlights
            TextRange textRange = new TextRange(this.OutputBox.Document.ContentStart, this.OutputBox.Document.ContentEnd);
            var sourceText = textRange.Text;

            int nonMatchStartIndex = 0;
            Paragraph matchHighlightSource = new Paragraph();
            foreach (var replacement in result.AppliedReplacements)
            {

                if (nonMatchStartIndex < replacement.Match.Index)
                {
                    var nonMatchText =
                        sourceText.Substring(
                            nonMatchStartIndex, replacement.Match.Index - nonMatchStartIndex);
                    matchHighlightSource.Inlines.Add(nonMatchText);
                }

                var matchText = replacement.Match.Value;
                var matchRun = new Run(matchText)
                { Background = Brushes.Chartreuse, ToolTip = replacement.Rule.SourcePattern };

                matchHighlightSource.Inlines.Add(matchRun);

                nonMatchStartIndex = replacement.Match.Index + replacement.Match.Length;
            }

            if (nonMatchStartIndex < sourceText.Length)
            {
                var nonMatchText =
                        sourceText.Substring(
                            nonMatchStartIndex);
                matchHighlightSource.Inlines.Add(nonMatchText);
            }

            this.OutputBox.Document.Blocks.Clear();
            this.OutputBox.Document.Blocks.Add(matchHighlightSource);
        }

        private void PopulateTargetBox(AutoEditResult result)
        {
            var editedSourceText = result.Result;

            int nonMatchStartIndex = 0;
            Paragraph matchHighlightSource = new Paragraph();
            foreach (var replacement in result.AppliedReplacements)
            {

                if (nonMatchStartIndex < replacement.OutputIndex)
                {
                    var nonMatchText =
                        editedSourceText.Substring(
                            nonMatchStartIndex, replacement.OutputIndex - nonMatchStartIndex);
                    matchHighlightSource.Inlines.Add(nonMatchText);
                }

                var matchText = replacement.Output;
                var matchRun = new Run(matchText)
                    { Background = Brushes.Chartreuse, ToolTip = replacement.Rule.Replacement };

                matchHighlightSource.Inlines.Add(matchRun);

                nonMatchStartIndex = replacement.Match.Index + replacement.OutputLength;
            }

            if (nonMatchStartIndex < editedSourceText.Length)
            {
                var nonMatchText =
                        editedSourceText.Substring(
                            nonMatchStartIndex);
                matchHighlightSource.Inlines.Add(nonMatchText);
            }

            this.EditedOutputBox.Document.Blocks.Clear();
            this.EditedOutputBox.Document.Blocks.Add(matchHighlightSource);
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            this.CreatedRule =
                new AutoEditRule()
                {
                    SourcePattern = this.SourcePattern.Text,
                    OutputPattern = this.PostEditPattern.Text,
                    Replacement = this.PostEditReplacement.Text

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
