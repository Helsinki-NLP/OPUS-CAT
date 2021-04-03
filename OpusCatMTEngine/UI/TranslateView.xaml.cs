using OpusMTInterface;
using System;
using System.Collections.Generic;
using System.Linq;
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
    public partial class TranslateView : UserControl
    {

        private MTModel model;
        private List<List<Run>> sourceRuns;
        private List<List<Run>> targetRuns;

        internal string Title { get; set; }

        public TranslateView(MTModel selectedModel)
        {
            this.Model = selectedModel;
            this.Title = String.Format(OpusCatMTEngine.Properties.Resources.Translate_TranslateTitle, Model.Name);
            InitializeComponent();
        }

        public MTModel Model { get => model; set => model = value; }

        private void translateButton_Click(object sender, RoutedEventArgs e)
        {
            TextRange textRange = new TextRange(this.SourceBox.Document.ContentStart, this.SourceBox.Document.ContentEnd);

            var source = textRange.Text;
            Task<List<TranslationPair>> translate =
                new Task<List<TranslationPair>>(() => OrderTranslation(source));
            translate.ContinueWith(x => Dispatcher.Invoke(() => this.PopulateBoxes(x.Result)));
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
                    //TODO: add language selectors to the translate window for multilingual models
                    translation.Add(
                        this.Model.Translate(
                            line,
                            this.Model.SourceLanguages.First(), this.Model.TargetLanguages.First()).Result);
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
                    pairindex,
                    this.targetRuns);
                
                this.sourceRuns.Add(sourcerunlist);
                sourcepara.Inlines.AddRange(sourcerunlist);
                this.SourceBox.Document.Blocks.Add(sourcepara);

                var targetpara = new Paragraph();
                var targetrunlist = this.GenerateRuns(
                    pair.SegmentedTranslation,
                    pair.SegmentedAlignmentTargetToSource,
                    pairindex,
                    this.sourceRuns);

                this.targetRuns.Add(targetrunlist);
                targetpara.Inlines.AddRange(targetrunlist);
                this.TargetBox.Document.Blocks.Add(targetpara);
            }
        }

        private List<Run> GenerateRuns(
            string[] tokens,
            Dictionary<int,List<int>> alignment,
            int translationIndex,
            List<List<Run>> otherRuns)
        {
            var runlist = new List<Run>();
            foreach (var (token, index) in tokens.Select((x, i) => (x, i)))
            {
                var tokenrun = new Run(token);
                if (alignment.ContainsKey(index))
                {
                    var alignedTokens = alignment[index];
                    tokenrun.MouseEnter += (x, y) => Tokenrun_MouseEnter(alignedTokens, otherRuns, translationIndex, x, y);
                    tokenrun.MouseLeave += (x, y) => Tokenrun_MouseLeave(alignedTokens, otherRuns, translationIndex, x, y);
                    
                }
                runlist.Add(tokenrun);
            }
            return runlist;
        }

        private void Tokenrun_MouseLeave(List<int> alignedTokens, List<List<Run>> otherRuns, int pairindex,object sender, MouseEventArgs e)
        {
            Run tokenrun = sender as Run;
            tokenrun.Background = Brushes.Transparent;
            var runlist = otherRuns[pairindex];
            foreach (var tokenIndex in alignedTokens)
            {
                runlist[tokenIndex].Background = Brushes.Transparent;
            }
        }

        private void Tokenrun_MouseEnter(List<int> alignedTokens, List<List<Run>> otherRuns, int pairindex, object sender, MouseEventArgs e)
        {
            Run tokenrun = sender as Run;
            tokenrun.Background = Brushes.BlueViolet;
            var runlist = otherRuns[pairindex];
            foreach (var tokenIndex in alignedTokens)
            {
                runlist[tokenIndex].Background = Brushes.BlueViolet;
            }
        }
    }
}
