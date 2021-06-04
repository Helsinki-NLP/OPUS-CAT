using OpusMTInterface;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.ServiceModel;
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
using System.Windows.Threading;

namespace OpusCatMTEngine
{
    /// <summary>
    /// Interaction logic for TranslateWindow.xaml
    /// </summary>
    public partial class TranslateView : UserControl, INotifyPropertyChanged
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
        private List<List<Run>> sourceRuns;
        private List<List<Run>> targetRuns;

        private IsoLanguage sourceLanguage;
        private IsoLanguage targetLanguage;
        private bool _showSegmentation;
        private List<Run> wordsplitList;

        internal string Title { get; set; }

        public TranslateView(MTModel selectedModel)
        {
            this.Model = selectedModel;
            this.DataContext = selectedModel;
            this.wordsplitList = new List<Run>();
            this.SourceLanguage = this.Model.SourceLanguages.First();
            this.TargetLanguage = this.Model.TargetLanguages.First();
            this.Title = String.Format(OpusCatMTEngine.Properties.Resources.Translate_TranslateTitle, Model.Name);
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
            get => _showSegmentation;
            set
            {
                _showSegmentation = value;
                if (value)
                {
                    foreach (var wordsplit in this.wordsplitList)
                    {
                        wordsplit.Text = "|";
                        wordsplit.Foreground = Brushes.Red;
                    }
                }
                else
                {
                    foreach (var wordsplit in this.wordsplitList)
                    {
                        wordsplit.Text = "";
                    }
                }
                NotifyPropertyChanged();
            }
        }

        private void translateButton_Click(object sender, RoutedEventArgs e)
        {
            TextRange textRange = new TextRange(this.SourceBox.Document.ContentStart, this.SourceBox.Document.ContentEnd);
            this.wordsplitList.Clear();
            var source = textRange.Text;
            Task<List<TranslationPair>> translate =
                new Task<List<TranslationPair>>(() => OrderTranslation(source));
            translate.ContinueWith(x => Dispatcher.Invoke(() =>
                {
                    if (x.Exception != null)
                    {
                        Log.Error(x.Exception.ToString());
                        //MessageBox.Show(x.Exception.ToString());
                        var errorRun = new Run("Error in the translation service, see log file for details");
                        errorRun.Background = Brushes.Red;
                        errorRun.Foreground = Brushes.White;
                        var errorBlock = new Paragraph(errorRun);
                        this.TargetBox.Document.Blocks.Clear();
                        this.TargetBox.Document.Blocks.Add(errorBlock);
                    }
                    else
                    {
                        this.PopulateBoxes(x.Result);
                    }
                }));
            translate.Start();
        }

        private List<TranslationPair> OrderTranslation(string source)
        {
            var translation = new List<TranslationPair>();
            using (System.IO.StringReader reader = new System.IO.StringReader(source))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    translation.Add(
                        this.Model.Translate(
                            line,
                            this.SourceLanguage, this.TargetLanguage).Result);
                }
            }

            return translation;
        }

        private void PopulateBoxes(List<TranslationPair> translation)
        {
            this.TargetBox.Document.Blocks.Clear();
            this.SourceBox.Document.Blocks.Clear();
            this.sourceRuns = new List<List<Run>>();
            this.targetRuns = new List<List<Run>>();

            foreach (var (pair, pairindex) in translation.Select((x, i) => (x, i)))
            {
                var sourcepara = new Paragraph();
                var sourcerunlist = this.GenerateRuns(
                    pair.SegmentedSourceSentence,
                    pair.SegmentedAlignmentSourceToTarget,
                    pair.Segmentation,
                    pairindex,
                    this.targetRuns);

                this.sourceRuns.Add(sourcerunlist);
                sourcepara.Inlines.AddRange(sourcerunlist);
                this.SourceBox.Document.Blocks.Add(sourcepara);

                var targetpara = new Paragraph();
                var targetrunlist = this.GenerateRuns(
                    pair.SegmentedTranslation,
                    pair.SegmentedAlignmentTargetToSource,
                    pair.Segmentation,
                    pairindex,
                    this.sourceRuns);

                this.targetRuns.Add(targetrunlist);
                targetpara.Inlines.AddRange(targetrunlist);
                this.TargetBox.Document.Blocks.Add(targetpara);
            }
        }

        private List<Run> GenerateRuns(
            string[] tokens,
            Dictionary<int, List<int>> alignment,
            SegmentationMethod segmentation,
            int translationIndex,
            List<List<Run>> otherRuns)
        {
            var runlist = new List<Run>();
            //Change this to add each token as a run, and add empty runs for space and pipe runs for segmentation
            //visualization
            foreach (var (token, index) in tokens.Select((x, i) => (x, i)))
            {
                string processedToken = token;
                switch (segmentation)
                {
                    case SegmentationMethod.SentencePiece:
                        if (token.StartsWith("▁"))
                        {
                            processedToken = processedToken.Substring(1);
                            if (index != 0)
                            {
                                var spaceRun = new Run(" ");
                                spaceRun.Tag = "space";
                                runlist.Add(spaceRun);
                            }
                        }
                        else
                        {
                            var wordsplitRun = new Run("");
                            wordsplitRun.Tag = "wordsplit";
                            runlist.Add(wordsplitRun);
                            this.wordsplitList.Add(wordsplitRun);
                        }
                        break;
                    case SegmentationMethod.Bpe:
                        if (!token.StartsWith("@@"))
                        {
                            processedToken = processedToken.Substring(2);
                            var wordsplitRun = new Run("");
                            wordsplitRun.Tag = "wordsplit";
                            this.wordsplitList.Add(wordsplitRun);
                            runlist.Add(wordsplitRun);
                        }
                        else
                        {
                            var spaceRun = new Run(" ");
                            spaceRun.Tag = "space";
                            runlist.Add(spaceRun);
                        }
                        break;
                    default:
                        throw new Exception("Segmentation method not specified in translation.");
                }

                Run tokenrun = new Run(processedToken);

                if (alignment.ContainsKey(index))
                {
                    var alignedTokens = alignment[index];
                    tokenrun.MouseEnter += (x, y) => Tokenrun_MouseEnter(alignedTokens, otherRuns, translationIndex, x, y);
                    tokenrun.MouseLeave += (x, y) => Tokenrun_MouseLeave(alignedTokens, otherRuns, translationIndex, x, y);
                }
                
                runlist.Add(tokenrun);
            }

            //This shows segmentation if ShowSegmentation is true
            NotifyPropertyChanged("ShowSegmentation");
            return runlist;
        }

        private void Tokenrun_MouseLeave(List<int> alignedTokens, List<List<Run>> otherRuns, int pairindex, object sender, MouseEventArgs e)
        {
            Run tokenrun = sender as Run;
            tokenrun.Background = Brushes.Transparent;
            //Ignore space and wordsplit token, since they are not included in the alignment
            var runlist = otherRuns[pairindex].Where(x => x.Tag == null ||
                (x.Tag.ToString() != "space" && x.Tag.ToString() != "wordsplit")).ToList();
            foreach (var tokenIndex in alignedTokens)
            {
                runlist[tokenIndex].Background = Brushes.Transparent;
            }
        }

        private void Tokenrun_MouseEnter(List<int> alignedTokens, List<List<Run>> otherRuns, int pairindex, object sender, MouseEventArgs e)
        {
            Run tokenrun = sender as Run;
            tokenrun.Background = Brushes.BlueViolet;
            //Ignore space and wordsplit token, since they are not included in the alignment
            var runlist = otherRuns[pairindex].Where(x => x.Tag == null ||
                (x.Tag.ToString() != "space" && x.Tag.ToString() != "wordsplit")).ToList();
            foreach (var tokenIndex in alignedTokens)
            {
                runlist[tokenIndex].Background = Brushes.BlueViolet;
            }
        }
    }
}
