using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Threading;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia;
using System;
using System.Linq;
using System.Text;
using Avalonia.Interactivity;

namespace OpusCatMtEngine
{
    public partial class TestPostEditRuleControl : UserControl
    {
        public static readonly StyledProperty<AutoEditRuleCollection> RuleCollectionProperty =
            AvaloniaProperty.Register<TestPreEditRuleControl, AutoEditRuleCollection>(nameof(RuleCollection));

        public static readonly StyledProperty<string> TitleProperty =
            AvaloniaProperty.Register<TestPreEditRuleControl, String>(nameof(Title));

        public static readonly StyledProperty<string> InputOriginProperty =
            AvaloniaProperty.Register<TestPreEditRuleControl, String>(nameof(InputOrigin));

        public static readonly StyledProperty<string> ButtonTextProperty =
            AvaloniaProperty.Register<TestPreEditRuleControl, String>(nameof(ButtonText));

        public static readonly StyledProperty<TextBox> SourcePatternBoxProperty =
            AvaloniaProperty.Register<TestPreEditRuleControl, TextBox>(nameof(SourcePatternBox));

        public static readonly StyledProperty<TextBox> PostEditReplacementBoxProperty =
            AvaloniaProperty.Register<TestPreEditRuleControl, TextBox>(nameof(PostEditReplacementBox));

        public static readonly StyledProperty<CheckBox> SourcePatternIsRegexProperty =
            AvaloniaProperty.Register<TestPreEditRuleControl, CheckBox>(nameof(SourcePatternIsRegex));

        public static readonly StyledProperty<TextBox> PostEditPatternBoxProperty =
            AvaloniaProperty.Register<TestPreEditRuleControl, TextBox>(nameof(PostEditPatternBox));

        public static readonly StyledProperty<CheckBox> PostEditPatternIsRegexProperty =
            AvaloniaProperty.Register<TestPreEditRuleControl, CheckBox>(nameof(PostEditPatternIsRegex));

        public static readonly StyledProperty<string> InputBoxLabelProperty =
            AvaloniaProperty.Register<TestPreEditRuleControl, string>(nameof(InputBoxLabel));

        public static readonly StyledProperty<bool> TestButtonVisibilityProperty =
            AvaloniaProperty.Register<TestPreEditRuleControl, bool>(nameof(TestButtonVisibility));

        public static readonly StyledProperty<bool> SourceBoxVisibilityProperty =
            AvaloniaProperty.Register<TestPreEditRuleControl, bool>(nameof(SourceBoxVisibility));


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
                this.SourceBoxVisibility = true;
            }
            else
            {
                this.SourceBoxVisibility = false;
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

        public bool TestButtonVisibility
        {
            get => GetValue(TestButtonVisibilityProperty);
            set => SetValue(TestButtonVisibilityProperty, value);
        }

        public bool SourceBoxVisibility
        {
            get => GetValue(SourceBoxVisibilityProperty);
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
                return this.SourceBox.Text;
            }
            set
            {
                this.SourceBox.Text = value;
            }
        }

        public string OutputText
        {
            get
            {
                //TODO: check if this works, if not use the code below
                return this.OutputBox.Inlines.Text;

                StringBuilder outputTextBuilder = new StringBuilder();
                foreach (var inline in this.OutputBox.Inlines)
                {
                    outputTextBuilder.Append(((Run)inline).Text);
                }

                return outputTextBuilder.ToString();
            }
            set
            {
                this.OutputBox.Text = value;
            }

        }

        public string EditedOutputText
        {
            get
            {
                //TODO: check if this works, if not use the code below
                return this.EditedOutputBox.Inlines.Text;

                StringBuilder outputTextBuilder = new StringBuilder();
                foreach (var inline in this.EditedOutputBox.Inlines)
                {
                    outputTextBuilder.Append(((Run)inline).Text);
                }

                return outputTextBuilder.ToString();
            }

        }

        public bool handlersAssigned = false;
        private bool sourceBoxDefaultVisibility;

        public TestPostEditRuleControl()
        {
            this.DataContext = this;
            InitializeComponent();

            //Need to add a handler to source pattern box after the dependency property has been set,
            //this will do it after all other rendering is complete
            //TODO: check that this works
            Dispatcher.UIThread.Invoke(new Action(() => SetHandlers()), DispatcherPriority.ContextIdle);
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

        public async void ProcessRules()
        {
            this.TestActive = false;
   
            try
            {
                var result = this.RuleCollection.ProcessPostEditRules(this.SourceText, this.OutputText);
                if (this.RuleCollection.EditRules.Any(x => !String.IsNullOrWhiteSpace(x.SourcePattern)))
                {
                    this.PopulateSourceBox(result);
                }
                this.PopulateOutputBox(result);
                this.PopulateTargetBox(result);
            }
            catch (ArgumentException ex)
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Invalid regular expression",
                                 $"Error in regular expression: {ex.Message}",
                                 ButtonEnum.Ok);
                await box.ShowAsync();
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
            int nonMatchStartIndex = 0;
            
            var nonRepeatedSourceMatches = result.AppliedReplacements.Where(x => !x.RepeatedSourceMatch);

            if (nonRepeatedSourceMatches.Any())
            {
                this.SourceBoxVisibility = true;
                foreach (var replacement in nonRepeatedSourceMatches)
                {
                    if (replacement.SourceMatch == null)
                    {
                        continue;
                    }

                    if (nonMatchStartIndex < replacement.SourceMatch.Index)
                    {
                        var nonMatchText =
                            this.SourceText.Substring(
                                nonMatchStartIndex, replacement.SourceMatch.Index - nonMatchStartIndex);
                        this.SourceBox.Inlines.Add(nonMatchText);
                    }

                    var matchText = replacement.SourceMatch.Value;
                    var matchRun = new Run(matchText)
                    {
                        Background = replacement.MatchColor,
                    };

                    this.SourceBox.Inlines.Add(matchRun);

                    nonMatchStartIndex = replacement.SourceMatch.Index + replacement.SourceMatch.Length;
                }

                if (nonMatchStartIndex < this.SourceText.Length)
                {
                    var nonMatchText =
                            this.SourceText.Substring(
                                nonMatchStartIndex);
                    this.SourceBox.Inlines.Add(nonMatchText);
                }

            }
            else
            {
                this.SourceBoxVisibility = this.sourceBoxDefaultVisibility;
            }
        }

        private void PopulateOutputBox(AutoEditResult result)
        {

            int nonMatchStartIndex = 0;
            foreach (var replacement in result.AppliedReplacements)
            {

                if (nonMatchStartIndex < replacement.Match.Index)
                {
                    var nonMatchText =
                        this.OutputText.Substring(
                            nonMatchStartIndex, replacement.Match.Index - nonMatchStartIndex);
                    this.OutputBox.Inlines.Add(nonMatchText);
                }

                var matchText = replacement.Match.Value;
                var matchRun = new Run(matchText)
                { Background = replacement.MatchColor };

                this.OutputBox.Inlines.Add(matchRun);

                nonMatchStartIndex = replacement.Match.Index + replacement.Match.Length;
            }

            if (nonMatchStartIndex < this.OutputText.Length)
            {
                var nonMatchText =
                        this.OutputText.Substring(
                            nonMatchStartIndex);
                this.OutputBox.Inlines.Add(nonMatchText);
            }

            this.RulesAppliedRun.Text = $"(rules applied: {result.AppliedReplacements.Count})";
        }

        private void PopulateTargetBox(AutoEditResult result)
        {
            var editedSourceText = result.Result;

            int nonMatchStartIndex = 0;

            foreach (var replacement in result.AppliedReplacements)
            {

                if (nonMatchStartIndex < replacement.OutputIndex)
                {
                    var nonMatchText =
                        editedSourceText.Substring(
                            nonMatchStartIndex, replacement.OutputIndex - nonMatchStartIndex);
                    this.EditedOutputBox.Inlines.Add(nonMatchText);
                }

                var matchText = replacement.Output;
                var matchRun = new Run(matchText)
                { Background = replacement.MatchColor};

                this.EditedOutputBox.Inlines.Add(matchRun);

                nonMatchStartIndex = replacement.OutputIndex + replacement.OutputLength;
            }

            if (nonMatchStartIndex < editedSourceText.Length)
            {
                var nonMatchText =
                        editedSourceText.Substring(
                            nonMatchStartIndex);
                this.EditedOutputBox.Inlines.Add(nonMatchText);
            }

        }

        private void AnyControl_TextChanged(object sender, EventArgs e)
        {
            //Changes to source pattern box may change visibility of source box in tester
            if (sender == this.SourcePatternBox)
            {
                if (String.IsNullOrEmpty(this.SourcePatternBox.Text))
                {
                    this.SourceBoxVisibility = false;
                }
                else
                {
                    this.SourceBoxVisibility = true;
                }
            }

            if (this.TestActive)
            {
                //Keep this on top, otherwise endless loop
                this.TestActive = false;

                //Clear edited output
                this.EditedOutputBox.Inlines.Clear();
                this.RulesAppliedRun.Text = "";

                //Remove highlights from source and unedited output
                //TODO: add the highlighted alternative boxes here, they are implemented only for pre-edit
                //tester at the moment
                this.SourceBox.Text = this.SourceText;

                this.OutputBox.Text = this.OutputText;

            }
        }

    }
}
