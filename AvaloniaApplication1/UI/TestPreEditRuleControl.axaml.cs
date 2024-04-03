using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Interactivity;
using Avalonia.Media.TextFormatting;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia;
using System;
using System.Text;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace OpusCatMtEngine
{
    public partial class TestPreEditRuleControl : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public static readonly StyledProperty<AutoEditRuleCollection> RuleCollectionProperty = 
            AvaloniaProperty.Register<TestPreEditRuleControl,AutoEditRuleCollection>(nameof(RuleCollection));

        public static readonly StyledProperty<string> TitleProperty = 
            AvaloniaProperty.Register<TestPreEditRuleControl,String>(nameof(Title));

        public static readonly StyledProperty<string> InputOriginProperty =
            AvaloniaProperty.Register<TestPreEditRuleControl, String>(nameof(InputOrigin));

        public static readonly StyledProperty<string> ButtonTextProperty =
            AvaloniaProperty.Register<TestPreEditRuleControl, String>(nameof(ButtonText));

        public static readonly StyledProperty<TextBox> PreEditReplacementBoxProperty =
            AvaloniaProperty.Register<TestPreEditRuleControl, TextBox>(nameof(PreEditReplacementBox));

        public static readonly StyledProperty<TextBox> PreEditPatternBoxProperty =
            AvaloniaProperty.Register<TestPreEditRuleControl, TextBox>(nameof(PreEditPatternBox));

        public static readonly StyledProperty<CheckBox> SourcePatternIsRegexProperty =
            AvaloniaProperty.Register<TestPreEditRuleControl, CheckBox>(nameof(SourcePatternIsRegex));

        public static readonly StyledProperty<string> InputBoxLabelProperty =
            AvaloniaProperty.Register<TestPreEditRuleControl, string>(nameof(InputBoxLabel));

        public static readonly StyledProperty<bool> TestButtonVisibilityProperty =
            AvaloniaProperty.Register<TestPreEditRuleControl, bool>(nameof(TestButtonVisibility));

        public bool TestActive
        {
            get => testActive;
            set
            {
                testActive = value;
                NotifyPropertyChanged();
            }
        }
        public AutoEditRuleCollection RuleCollection
        {
            get => (AutoEditRuleCollection)GetValue(RuleCollectionProperty);
            set => SetValue(RuleCollectionProperty, value);
        }

        public TextBox PreEditPatternBox
        {
            get => (TextBox)GetValue(PreEditPatternBoxProperty);
            set => SetValue(PreEditPatternBoxProperty, value);
        }

        public CheckBox SourcePatternIsRegex
        {
            get => (CheckBox)GetValue(SourcePatternIsRegexProperty);
            set
            {
                SetValue(SourcePatternIsRegexProperty, value);
            }
        }

        public TextBox PreEditReplacementBox
        {
            get => (TextBox)GetValue(PreEditReplacementBoxProperty);
            set => SetValue(PreEditReplacementBoxProperty, value);
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
                return this.EditedSourceBox.Inlines.Text;

                StringBuilder outputTextBuilder = new StringBuilder();
                foreach (var inline in this.EditedSourceBox.Inlines)
                {
                    outputTextBuilder.Append(((Run)inline).Text);
                }
                
                return outputTextBuilder.ToString();
            }

        }

        public bool textBoxHandlersAssigned = false;
        private bool testActive;

        public TestPreEditRuleControl()
        {
            this.DataContext = this;
            InitializeComponent();
            this.SourceBox.TextChanged += SourceBox_TextChanged;
        }

        private void SourceBox_TextChanged(object? sender, TextChangedEventArgs e)
        {
            AnyControl_TextChanged(sender, e);
        }

        public async void ProcessRules()
        {
            this.TestActive = false;
            
            try
            {
                var result = this.RuleCollection.ProcessPreEditRules(this.SourceText);
                this.PopulateSourceBox(result);
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

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            AnyControl_TextChanged(sender, e);
        }

        private void PreEditTest_Click(object sender, RoutedEventArgs e)
        {
            //If these have been defined, generate rule collection from them
            if (this.PreEditPatternBox != null && this.PreEditReplacementBox != null)
            {
                this.RuleCollection = new AutoEditRuleCollection();
                this.RuleCollection.AddRule(
                    new AutoEditRule()
                    {
                        SourcePattern = this.PreEditPatternBox.Text,
                        SourcePatternIsRegex = this.SourcePatternIsRegex.IsChecked.Value,
                        Replacement = this.PreEditReplacementBox.Text
                    });
                if (!this.textBoxHandlersAssigned)
                {
                    this.PreEditPatternBox.TextChanged += this.AnyControl_TextChanged;
                    this.PreEditReplacementBox.TextChanged += this.AnyControl_TextChanged;
                    this.SourcePatternIsRegex.Checked += this.AnyControl_TextChanged;
                    this.SourcePatternIsRegex.Unchecked += this.AnyControl_TextChanged;
                    this.textBoxHandlersAssigned = true;
                }
            }

            this.ProcessRules();

        }

        private void PopulateSourceBox(AutoEditResult result)
        {
            int nonMatchStartIndex = 0;
            
            foreach (var replacement in result.AppliedReplacements)
            {

                if (nonMatchStartIndex < replacement.Match.Index)
                {
                    var nonMatchText =
                        this.SourceText.Substring(
                            nonMatchStartIndex, replacement.Match.Index - nonMatchStartIndex);
                    this.SourceHighlightBox.Inlines.Add(nonMatchText);
                }

                var matchText = replacement.Match.Value;

                var matchRun = new Run(matchText)
                { Background = replacement.MatchColor };
                //TODO: use HyperLinkButton to add Tooltip
                //{ Background = replacement.MatchColor, ToolTip = replacement.Rule.SourcePattern };

                this.SourceHighlightBox.Inlines.Add(matchRun);

                nonMatchStartIndex = replacement.Match.Index + replacement.Match.Length;
            }

            if (nonMatchStartIndex < this.SourceText.Length)
            {
                var nonMatchText =
                        this.SourceText.Substring(
                            nonMatchStartIndex);
                this.SourceHighlightBox.Inlines.Add(nonMatchText);
            }

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
                    this.EditedSourceBox.Inlines.Add(nonMatchText);
                }

                var matchText = replacement.Output;
                var matchRun = new Run(matchText)
                { Background = replacement.MatchColor };

                this.EditedSourceBox.Inlines.Add(matchRun);

                nonMatchStartIndex = replacement.OutputIndex + replacement.OutputLength;
            }

            if (nonMatchStartIndex < editedSourceText.Length)
            {
                var nonMatchText =
                        editedSourceText.Substring(
                            nonMatchStartIndex);
                this.EditedSourceBox.Inlines.Add(nonMatchText);
            }


            this.RulesAppliedRun.Text = $"(rules applied: {result.AppliedReplacements.Count})";

        }



        public void AnyControl_TextChanged(object? sender, RoutedEventArgs e)
        {
            if (this.TestActive)
            {
                //Keep this on top, otherwise endless loop
                this.TestActive = false;

                //Clear edited output
                this.EditedSourceBox.Inlines.Clear();

                this.SourceHighlightBox.Inlines.Clear();
                this.RulesAppliedRun.Text = "";

            }
        }
    }
}
