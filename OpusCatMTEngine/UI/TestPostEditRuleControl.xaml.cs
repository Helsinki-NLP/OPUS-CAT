using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace OpusCatMtEngine
{
    /// <summary>
    /// Interaction logic for TestPostEditRuleControl.xaml
    /// </summary>
    public partial class TestPostEditRuleControl : UserControl
    {
        public static readonly DependencyProperty RuleCollectionProperty = DependencyProperty.Register(
            "RuleCollection", typeof(AutoEditRuleCollection),
            typeof(TestPostEditRuleControl)
            );

        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
            "Title", typeof(string),
            typeof(TestPostEditRuleControl)
            );

        public static readonly DependencyProperty InputOriginProperty = DependencyProperty.Register(
           "InputOrigin", typeof(string),
           typeof(TestPostEditRuleControl)
           );


        public static readonly DependencyProperty ButtonTextProperty = DependencyProperty.Register(
            "ButtonText", typeof(string),
            typeof(TestPostEditRuleControl)
            );

        public static readonly DependencyProperty SourcePatternBoxProperty = DependencyProperty.Register(
            "SourcePatternBox", typeof(TextBox),
            typeof(TestPostEditRuleControl)
            );

        public static readonly DependencyProperty SourcePatternIsRegexProperty = DependencyProperty.Register(
            "SourcePatternIsRegex", typeof(CheckBox),
            typeof(TestPostEditRuleControl)
            );


        public static readonly DependencyProperty PostEditReplacementBoxProperty = DependencyProperty.Register(
            "PostEditReplacementBox", typeof(TextBox),
            typeof(TestPostEditRuleControl)
            );

        public static readonly DependencyProperty PostEditPatternBoxProperty = DependencyProperty.Register(
            "PostEditPatternBox", typeof(TextBox),
            typeof(TestPostEditRuleControl)
            );

        public static readonly DependencyProperty PostEditPatternIsRegexProperty = DependencyProperty.Register(
            "PostEditPatternIsRegex", typeof(CheckBox),
            typeof(TestPostEditRuleControl)
            );

        public static readonly DependencyProperty InputBoxLabelProperty = DependencyProperty.Register(
            "InputBoxLabel", typeof(string),
            typeof(TestPostEditRuleControl), new UIPropertyMetadata("Input to collection: MT output")
            );

        public static readonly DependencyProperty TestButtonVisibilityProperty = DependencyProperty.Register(
            "TestButtonVisibility", typeof(Visibility),
            typeof(TestPostEditRuleControl), new UIPropertyMetadata(Visibility.Visible)
            );

        public static readonly DependencyProperty SourceBoxVisibilityProperty = DependencyProperty.Register(
            "SourceBoxVisibility", typeof(Visibility),
            typeof(TestPostEditRuleControl), new UIPropertyMetadata(Visibility.Visible)
            );

        public bool TestActive { get; private set; }
        public AutoEditRuleCollection RuleCollection
        {
            get => (AutoEditRuleCollection)GetValue(RuleCollectionProperty);
            set
            {
                SetValue(RuleCollectionProperty, value);
            }
        }

        internal void Refresh()
        {
            if (this.RuleCollection.EditRules.Any(x => !String.IsNullOrEmpty(x.SourcePattern)))
            {
                this.SourceBoxVisibility = Visibility.Visible;
            }
            else
            {
                this.SourceBoxVisibility = Visibility.Collapsed;
            }
        }

        public TextBox SourcePatternBox
        {
            get => (TextBox)GetValue(SourcePatternBoxProperty);
            set
            {
                SetValue(SourcePatternBoxProperty, value);
            }
            
        }

        public CheckBox SourcePatternIsRegex
        {
            get => (CheckBox)GetValue(SourcePatternIsRegexProperty);
            set
            {
                SetValue(SourcePatternIsRegexProperty, value);
            }

        }

        public TextBox PostEditPatternBox
        {
            get => (TextBox)GetValue(PostEditPatternBoxProperty);
            set => SetValue(PostEditPatternBoxProperty, value);
        }

        public CheckBox PostEditPatternIsRegex
        {
            get => (CheckBox)GetValue(PostEditPatternIsRegexProperty);
            set
            {
                SetValue(PostEditPatternIsRegexProperty, value);
            }

        }

        public TextBox PostEditReplacementBox
        {
            get => (TextBox)GetValue(PostEditReplacementBoxProperty);
            set => SetValue(PostEditReplacementBoxProperty, value);
        }

        public string ButtonText
        {
            get => (string)GetValue(ButtonTextProperty);
            set => SetValue(ButtonTextProperty, value);
        }

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public string InputOrigin
        {
            get => (string)GetValue(InputOriginProperty);
            set => SetValue(InputOriginProperty, value);
        }



        public string InputBoxLabel
        {
            get => (string)GetValue(InputBoxLabelProperty);
            set => SetValue(InputBoxLabelProperty, value);
        }

        public Visibility TestButtonVisibility
        {
            get => (Visibility)GetValue(TestButtonVisibilityProperty);
            set => SetValue(TestButtonVisibilityProperty, value);
        }

        public Visibility SourceBoxVisibility
        {
            get => (Visibility)GetValue(SourceBoxVisibilityProperty);
            set
            {
                this.sourceBoxDefaultVisibility = value;
                SetValue(SourceBoxVisibilityProperty, value);
            }
        }

        public string SourceText
        {
            get
            {
                TextRange textRange = new TextRange(this.SourceBox.Document.ContentStart, this.SourceBox.Document.ContentEnd);
                var sourceText = textRange.Text.Trim('\r', '\n');
                return sourceText;
            }
            set
            {
                Paragraph sourcePara = new Paragraph();
                sourcePara.Inlines.Add(new Run(value.Trim('\r', '\n')));
                this.SourceBox.Document.Blocks.Clear();
                this.SourceBox.Document.Blocks.Add(sourcePara);
            }
        }

        public string OutputText
        {
            get
            {
                TextRange textRange = new TextRange(this.OutputBox.Document.ContentStart, this.OutputBox.Document.ContentEnd);
                var outputText = textRange.Text.Trim('\r', '\n');
                return outputText;
            }
            set
            {
                Paragraph outputPara = new Paragraph();
                outputPara.Inlines.Add(new Run(value));
                foreach (var block in this.OutputBox.Document.Blocks.ToList())
                {
                    this.OutputBox.Document.Blocks.Remove(block);
                }
                
                this.OutputBox.Document.Blocks.Add(outputPara);
            }
        }
        
        public string EditedOutputText
        {
            get
            {
                TextRange textRange = new TextRange(this.EditedOutputBox.Document.ContentStart, this.EditedOutputBox.Document.ContentEnd);
                var editedOutputText = textRange.Text.Trim('\r', '\n');
                return editedOutputText;
            }

        }

        public bool handlersAssigned = false;
        private Visibility sourceBoxDefaultVisibility;

        public TestPostEditRuleControl()
        {
            this.DataContext = this;
            InitializeComponent();

            //Need to add a handler to source pattern box after the dependency property has been set,
            //this will do it after all other rendering is complete
            Dispatcher.BeginInvoke(new Action(() => SetHandlers()), DispatcherPriority.ContextIdle, null);
        }

        private void SetHandlers()
        {
            if (this.SourcePatternBox != null)
            {
                this.SourcePatternBox.TextChanged += AnyControl_TextChanged;
                this.SourcePatternIsRegex.Checked += AnyControl_TextChanged;
                this.SourcePatternIsRegex.Unchecked += AnyControl_TextChanged;
                AnyControl_TextChanged(this.SourcePatternBox, null);
            }
        }

        public void ProcessRules()
        {
            this.TestActive = false;
            TextRange sourceTextRange = new TextRange(this.SourceBox.Document.ContentStart, this.SourceBox.Document.ContentEnd);
            var sourceText = sourceTextRange.Text.Trim('\r', '\n');

            TextRange outputTextRange = new TextRange(this.OutputBox.Document.ContentStart, this.OutputBox.Document.ContentEnd);
            var outputText = outputTextRange.Text.Trim('\r', '\n');
            try
            {
                var result = this.RuleCollection.ProcessPostEditRules(sourceText, outputText);
                if (this.RuleCollection.EditRules.Any(x => !String.IsNullOrWhiteSpace(x.SourcePattern)))
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

            this.TestActive = true;
        }

        private void PostEditTest_Click(object sender, RoutedEventArgs e)
        {
            if (this.PostEditPatternBox != null && this.PostEditReplacementBox != null)
            {
                this.RuleCollection = new AutoEditRuleCollection();

                this.RuleCollection.AddRule(
                    new AutoEditRule()
                    {
                        SourcePattern = this.SourcePatternBox.Text,
                        SourcePatternIsRegex = this.SourcePatternIsRegex.IsChecked.Value,
                        OutputPattern = this.PostEditPatternBox.Text,
                        OutputPatternIsRegex = this.PostEditPatternIsRegex.IsChecked.Value,
                        Replacement = this.PostEditReplacementBox.Text
                    });
                if (!this.handlersAssigned)
                {
                    this.PostEditPatternBox.TextChanged += this.AnyControl_TextChanged;
                    this.PostEditReplacementBox.TextChanged += this.AnyControl_TextChanged;
                    this.PostEditPatternIsRegex.Checked += this.AnyControl_TextChanged;
                    this.PostEditPatternIsRegex.Unchecked += this.AnyControl_TextChanged;
                    this.handlersAssigned = true;
                }
            }

            this.ProcessRules();
        }

        private void PopulateSourceBox(AutoEditResult result)
        {
            //Store the source text, use it as basis of the source text with match highlights
            TextRange textRange = new TextRange(this.SourceBox.Document.ContentStart, this.SourceBox.Document.ContentEnd);
            var sourceText = textRange.Text.Trim('\r', '\n');

            int nonMatchStartIndex = 0;
            Paragraph matchHighlightSource = new Paragraph();

            var nonRepeatedSourceMatches = result.AppliedReplacements.Where(x => !x.RepeatedSourceMatch);

            if (nonRepeatedSourceMatches.Any())
            {
                this.SourceBoxVisibility = Visibility.Visible;
                foreach (var replacement in nonRepeatedSourceMatches)
                {
                    if (replacement.SourceMatch == null)
                    {
                        continue;
                    }

                    if (nonMatchStartIndex < replacement.SourceMatch.Index)
                    {
                        var nonMatchText =
                            sourceText.Substring(
                                nonMatchStartIndex, replacement.SourceMatch.Index - nonMatchStartIndex);
                        matchHighlightSource.Inlines.Add(nonMatchText);
                    }

                    var matchText = replacement.SourceMatch.Value;
                    var matchRun = new Run(matchText)
                    {
                        Background = replacement.MatchColor,
                        ToolTip = replacement.Rule.SourcePattern
                    };

                    matchHighlightSource.Inlines.Add(matchRun);

                    nonMatchStartIndex = replacement.SourceMatch.Index + replacement.SourceMatch.Length;
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
            else
            {
                this.SourceBoxVisibility = this.sourceBoxDefaultVisibility;
            }
        }

        private void PopulateOutputBox(AutoEditResult result)
        {
            //Store the source text, use it as basis of the source text with match highlights
            TextRange textRange = new TextRange(this.OutputBox.Document.ContentStart, this.OutputBox.Document.ContentEnd);
            var outputText = textRange.Text.Trim('\r', '\n');

            int nonMatchStartIndex = 0;
            Paragraph matchHighlightSource = new Paragraph();
            foreach (var replacement in result.AppliedReplacements)
            {

                if (nonMatchStartIndex < replacement.Match.Index)
                {
                    var nonMatchText =
                        outputText.Substring(
                            nonMatchStartIndex, replacement.Match.Index - nonMatchStartIndex);
                    matchHighlightSource.Inlines.Add(nonMatchText);
                }

                var matchText = replacement.Match.Value;
                var matchRun = new Run(matchText)
                { Background = replacement.MatchColor, ToolTip = replacement.Rule.OutputPattern };

                matchHighlightSource.Inlines.Add(matchRun);

                nonMatchStartIndex = replacement.Match.Index + replacement.Match.Length;
            }

            if (nonMatchStartIndex < outputText.Length)
            {
                var nonMatchText =
                        outputText.Substring(
                            nonMatchStartIndex);
                matchHighlightSource.Inlines.Add(nonMatchText);
            }

            this.RulesAppliedRun.Text = $"(rules applied: {result.AppliedReplacements.Count})";

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
                { Background = replacement.MatchColor, ToolTip = replacement.Rule.Replacement };

                matchHighlightSource.Inlines.Add(matchRun);

                nonMatchStartIndex = replacement.OutputIndex + replacement.OutputLength;
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

        private void AnyControl_TextChanged(object sender, EventArgs e)
        {
            //Changes to source pattern box may change visibility of source box in tester
            if (sender == this.SourcePatternBox)
            {
                if (String.IsNullOrEmpty(this.SourcePatternBox.Text))
                {
                    this.SourceBoxVisibility = Visibility.Collapsed;
                }
                else
                {
                    this.SourceBoxVisibility = Visibility.Visible;
                }
            }

            if (this.TestActive)
            {
                //Keep this on top, otherwise endless loop
                this.TestActive = false;

                //Clear edited output
                this.EditedOutputBox.Document.Blocks.Clear();
                this.RulesAppliedRun.Text = "";

                //Remove highlights from source and unedited output
                TextRange sourceTextRange = new TextRange(this.SourceBox.Document.ContentStart, this.SourceBox.Document.ContentEnd);
                var sourceText = sourceTextRange.Text.Trim('\r', '\n');
                var cleanSource = new Paragraph(new Run(sourceText));
                RichTextBoxHelper.UpdateRichTextBoxWithCaretInSamePosition(this.SourceBox, cleanSource);

                TextRange outputTextRange = new TextRange(this.OutputBox.Document.ContentStart, this.OutputBox.Document.ContentEnd);
                var outputText = outputTextRange.Text.Trim('\r', '\n');
                var cleanOutput = new Paragraph(new Run(outputText));
                RichTextBoxHelper.UpdateRichTextBoxWithCaretInSamePosition(this.OutputBox, cleanOutput);

            }
        }

    }
}
