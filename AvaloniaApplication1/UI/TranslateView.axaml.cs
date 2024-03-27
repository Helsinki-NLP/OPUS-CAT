using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using SentenceSplitterNet;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OpusCatMtEngine
{
    public partial class TranslateView : UserControl
    {
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private MTModel model;
        private List<List<Inline>> sourceRuns;
        private List<List<Inline>> targetRuns;

        private IsoLanguage sourceLanguage;
        private IsoLanguage targetLanguage;
        private bool _showSegmentation;
        private List<Inline> wordsplitList;

        internal string Title { get; set; }

        public TranslateView()
        {

        }

        public TranslateView(MTModel selectedModel)
        {
            
            this.Model = selectedModel;
            this.DataContext = selectedModel;
            this.wordsplitList = new List<Inline>();
            this.SourceLanguage = this.Model.SourceLanguages.First();
            this.TargetLanguage = this.Model.TargetLanguages.First();
            this.Title = String.Format(Properties.Resources.Translate_TranslateTitle, Model.Name);
            InitializeComponent();
        }

        public MTModel Model { get => model; set => model = value; }

        public IsoLanguage SourceLanguage
        {
            get => sourceLanguage;
            set
            {
                NotifyPropertyChanged();
                sourceLanguage = value;
            }
        }

        public IsoLanguage TargetLanguage
        {
            get => targetLanguage;
            set
            {
                NotifyPropertyChanged();
                targetLanguage = value;
            }
        }

        public bool ShowSegmentation
        {
            get
            {
                if (_showSegmentation)
                {
                    foreach (var wordsplit in this.wordsplitList)
                    {
                        var textblock = this.GetMousableRunTextBlock(wordsplit);
                        textblock.Text = "|";
                        textblock.Foreground = Brushes.Red;
                    }
                }
                else
                {
                    foreach (var wordsplit in this.wordsplitList)
                    {
                        var textblock = this.GetMousableRunTextBlock(wordsplit);
                        textblock.Text = "";
                    }
                }

                return _showSegmentation;
            }
            set
            {
                _showSegmentation = value;
                NotifyPropertyChanged();
            }
        }

        private TextBlock GetMousableRunTextBlock(Inline mousableRun)
        {
            return (TextBlock)this.GetMousableRunButton(mousableRun).Content;
        }

        private Button GetMousableRunButton(Inline mousableRun)
        {
            var container = (InlineUIContainer)mousableRun;
            return (Button)container.Child;
        }



        public bool AutoEditedTranslations
        {
            get => autoEditedTranslations;
            set
            {
                autoEditedTranslations = value;
                NotifyPropertyChanged();
            }
        }

        public bool TranslationActive 
        { 
            get => translationActive;
            set 
            {
                translationActive = value; 
                NotifyPropertyChanged();
            } 
        }

        private bool autoEditedTranslations;
        private bool translationActive;

        private async void ClearButtonClick(object? sender, RoutedEventArgs e)
        {
            //TODO: There's a bug in Avalonia that causes Inlines.Clear() not to work.
            //The bug has apparently been fixed in 1.1.0.8, but that version fails because of some
            //other exception (the control already has a visual parent).
            //Leave this until the last touches, and check if it's been fixed by then
            
            //WORKAROUND: Reuse the inline buttons instead of trying to clear and
            //recreate them. Maybe create x number of buttons beforehand so that they
            //can be used for any text. This would restrict the length of translations,
            //but that's not fatal. Leave this for last.

            
            this.TranslationActive = false;
            this.wordsplitList.Clear();
            this.sourceRuns.Clear();
            this.targetRuns.Clear();

            this.SourceBoxDisplay.Inlines.Clear();
        }

        private async void TranslateButtonClick(object? sender, RoutedEventArgs e)
        {
            this.AutoEditedTranslations = false;
            this.TargetBox.Inlines.Clear();

            //If the source text is segmented (happens if segmented text is retranslated),
            //set the segment split to empty before translating.
            foreach (var wordsplit in this.wordsplitList)
            {
                var textblock = this.GetMousableRunTextBlock(wordsplit);
                textblock.Text = "";
            }
            this.wordsplitList.Clear();
            try
            {
                var source = this.SourceInputBox.Text;
                var translate = await Task.Run(() => OrderTranslation(source));
                this.PopulateBoxes(translate);
                if (translate.Any(y => y.AutoEditedTranslation))
                {
                    this.AutoEditedTranslations = true;
                }
                this.TranslationActive = true;
            }
            catch (Exception)
            {
                //Log.Error(x.Exception.ToString());
                //MessageBox.Show(x.Exception.ToString());
                var errorRun = new Run("Error in the translation service, see log file for details");
                errorRun.Background = Brushes.Red;
                errorRun.Foreground = Brushes.White;
                this.TargetBox.Inlines.Clear();
                this.TargetBox.Inlines.Add(errorRun);
            }
            
            
        }

        private List<TranslationPair> OrderTranslation(string source)
        {
            var splitter = new SentenceSplitter(this.sourceLanguage.ShortestIsoCode);
            var translation = new List<TranslationPair>();
            using (System.IO.StringReader reader = new System.IO.StringReader(source))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    var sourceSentences = splitter.Split(line);
                    TranslationPair finalTranslation = null;
                    foreach (var sourceSentence in sourceSentences)
                    {
                        var translationPart = this.Model.Translate(sourceSentence, this.SourceLanguage, this.TargetLanguage);
                        
                        if (finalTranslation == null)
                        {
                            finalTranslation = translationPart.Result;
                        }
                        else
                        {
                            finalTranslation.AppendTranslationPair(translationPart.Result);
                        }
                    }
                    translation.Add(finalTranslation);
                }
            }

            return translation;
        }

        private void PopulateBoxes(List<TranslationPair> translation)
        {
            this.SourceBoxDisplay.Inlines.Clear();
            this.sourceRuns = new List<List<Inline>>();
            this.targetRuns = new List<List<Inline>>();

            bool firstTranslationPair = true;
            foreach (var (pair, pairindex) in translation.Select((x, i) => (x, i)))
            {
                if (firstTranslationPair)
                {
                    firstTranslationPair = false;
                }
                else
                {
                    SourceBoxDisplay.Inlines.Add(new Run("\n"));
                    TargetBox.Inlines.Add(new Run("\n"));
                }

                var sourcerunlist = this.GenerateRuns(
                    pair.SegmentedSourceSentence,
                    pair.SegmentedAlignmentSourceToTarget,
                    pair.Segmentation,
                    pairindex,
                    this.targetRuns,
                    pair.AutoEditedTranslation);

                this.sourceRuns.Add(sourcerunlist);
                this.SourceBoxDisplay.Inlines.AddRange(sourcerunlist);

                var targetrunlist = this.GenerateRuns(
                    pair.SegmentedTranslation,
                    pair.SegmentedAlignmentTargetToSource,
                    pair.Segmentation,
                    pairindex,
                    this.sourceRuns,
                    pair.AutoEditedTranslation);

                this.targetRuns.Add(targetrunlist);
                this.TargetBox.Inlines.AddRange(targetrunlist);
            }
        }

        private InlineUIContainer GenerateMousableInline(
            string text, TextDecorationCollection underline=null, string tag = null)
        {
            var container = new InlineUIContainer();
            var button = new Button();
            container.Child = button;
            var textBlock = new TextBlock();
            button.Content = textBlock;
            textBlock.Text = text;
            if (underline != null) 
            {
                textBlock.TextDecorations = underline;
            }
            if (tag != null)
            {
                button.Tag = tag;
            }

            return container;
        }

        private List<Inline> GenerateRuns(
            string[] tokens,
            Dictionary<int, List<int>> alignment,
            SegmentationMethod segmentation,
            int translationIndex,
            List<List<Inline>> otherRuns,
            bool autoEditedTranslation)
        {
            var runlist = new List<Inline>();
            //Change this to add each token as a run, and add empty runs for space and pipe runs for segmentation
            //visualization
            bool insideTerm = false;
            foreach (var (token, index) in tokens.Select((x, i) => (x, i)))
            {
                string processedToken = token;
                Inline tokenrun;
                switch (segmentation)
                {
                    case SegmentationMethod.SentencePiece:
                        if (token.StartsWith("▁"))
                        {

                            if (index != 0)
                            {
                                var spaceRun = this.GenerateMousableInline(" ", tag: "space");
                                runlist.Add(spaceRun);
                            }

                            processedToken = processedToken.Substring(1);
                            

                            if (insideTerm)
                            {
                                tokenrun = this.GenerateMousableInline(token,TextDecorations.Underline);
                            }
                            else
                            {
                                tokenrun = this.GenerateMousableInline(token);
                            }

                            runlist.Add(tokenrun);
                        }
                        else if (token == "<term_start>" || token == "<term_mask>")
                        {
                            tokenrun = new Run("");
                            runlist.Add(tokenrun);
                        }
                        else if (token == "<term_end>")
                        {
                            tokenrun = new Run("");
                            runlist.Add(tokenrun);
                            insideTerm = true;
                        }
                        else if (token == "<trans_end>")
                        {
                            tokenrun = new Run("");
                            runlist.Add(tokenrun);
                            insideTerm = false;
                        }
                        else
                        {
                            //TODO: wordsplits should not show up until the checkbox is selected,
                            //now they are shown as empty buttons, like spaces
                            var wordsplitRun = GenerateMousableInline("",tag:"wordsplit");
                            
                            runlist.Add(wordsplitRun);
                            this.wordsplitList.Add(wordsplitRun);
                            
                            if (insideTerm)
                            {
                                tokenrun = GenerateMousableInline(processedToken,TextDecorations.Underline);
                            }
                            else
                            {
                                tokenrun = GenerateMousableInline(processedToken);
                            }
                            runlist.Add(tokenrun);
                        }
                        break;
                    case SegmentationMethod.Bpe:
                        if (token.EndsWith("@@"))
                        {
                            processedToken = processedToken.Substring(0, processedToken.Length - 2);
                            tokenrun = new Run(processedToken);
                            runlist.Add(tokenrun);

                            var wordsplitRun = this.GenerateMousableInline("",tag:"wordsplit");
                            this.wordsplitList.Add(wordsplitRun);
                            runlist.Add(wordsplitRun);
                        }
                        else
                        {
                            tokenrun = this.GenerateMousableInline(processedToken);
                            runlist.Add(tokenrun);

                            if (index != tokens.Length - 1)
                            {
                                var spaceRun = this.GenerateMousableInline(" ", tag: "space");
                                runlist.Add(spaceRun);
                            }
                        }
                        break;
                    default:
                        throw new Exception("Segmentation method not specified in translation.");
                }


                if (this.model.SupportsWordAlignment && !autoEditedTranslation && alignment.ContainsKey(index))
                {
                    var alignedTokens = alignment[index];
                    this.GetMousableRunButton(tokenrun).PointerEntered += (x, y) => Tokenrun_MouseEnter(alignedTokens, otherRuns, translationIndex, x, y);
                    this.GetMousableRunButton(tokenrun).PointerExited += (x, y) => Tokenrun_MouseLeave(alignedTokens, otherRuns, translationIndex, x, y);
                }

            }

            //This shows segmentation if ShowSegmentation 
            NotifyPropertyChanged("ShowSegmentation");
            return runlist;
        }

        private void Tokenrun_MouseLeave(List<int> alignedTokens, List<List<Inline>> otherRuns, int pairindex, object sender, PointerEventArgs e)
        {
            var tokenTextBlock = (TextBlock)((Button)sender).Content;
            tokenTextBlock.Background = Brushes.Transparent;
            //Ignore space and wordsplit token, since they are not included in the alignment
            var runlist = otherRuns[pairindex].Where(x => this.GetMousableRunButton(x).Tag == null ||
                (this.GetMousableRunButton(x).Tag.ToString() != "space" && this.GetMousableRunButton(x).Tag.ToString() != "wordsplit")).ToList();
            foreach (var tokenIndex in alignedTokens)
            {
                this.GetMousableRunTextBlock(runlist[tokenIndex]).Background = Brushes.Transparent;
            }
        }

        private void Tokenrun_MouseEnter(List<int> alignedTokens, List<List<Inline>> otherRuns, int pairindex, object sender, PointerEventArgs e)
        {
            var tokenTextBlock = (TextBlock)((Button)sender).Content;
            tokenTextBlock.Background = Brushes.BlueViolet;
            //Ignore space and wordsplit token, since they are not included in the alignment
            var runlist = otherRuns[pairindex].Where(x => this.GetMousableRunButton(x).Tag == null ||
                (this.GetMousableRunButton(x).Tag.ToString() != "space" && this.GetMousableRunButton(x).Tag.ToString() != "wordsplit")).ToList();
            foreach (var tokenIndex in alignedTokens)
            {
                this.GetMousableRunTextBlock(runlist[tokenIndex]).Background = Brushes.BlueViolet;
            }
        }
    }
}
