using OpusMTInterface;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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
    /// Interaction logic for TestWindow.xaml
    /// </summary>
    public partial class TestView : UserControl, INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        /*private string sourceFilePath;
        public string SourceFilePath
        {
            get => sourceFilePath;
            set
            {
                sourceFilePath = value.Trim('"');
                NotifyPropertyChanged();
            }
        }*/



        private MTModel model;

        public TestView(MTModel selectedModel)
        {
            this.Model = selectedModel;
            this.Title = $"Translating with model {Model.Name}";
            InitializeComponent();
            var sourceCode = this.model.SourceLanguages.Single();
            var targetCode = this.model.TargetLanguages.Single();
            var testsets = Directory.GetDirectories("TatoebaTestsets");
            var testsetDir = testsets.Single(
                x => x.EndsWith($"{sourceCode}-{targetCode}") || x.EndsWith($"{targetCode}-{sourceCode}"));
            this.SourceFileBox.Text = Directory.GetFiles(testsetDir, $"tatoeba.{sourceCode}.txt").Select(x => new FileInfo(x)).Single().FullName;
            this.RefFileBox.Text = Directory.GetFiles(testsetDir, $"tatoeba.{targetCode}.txt").Select(x => new FileInfo(x)).Single().FullName;
            this.TargetFileBox.Text = this.SourceFileBox.Text.Replace(".txt", $"{this.model.Name}.txt");
        }

        public MTModel Model { get => model; set => model = value; }
        public string Title { get; private set; }

        private void DisplayFileDialog()
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".txt"; // Default file extension
            //dlg.Filter = "Txt files (.txt)|*.txt"; // Filter files by extension

            // Show open file dialog box
            bool? result = dlg.ShowDialog();

            if (result == true)
            {

            }
        }

        private void btnAddSourceFile_Click(object sender, RoutedEventArgs e)
        {

        }

        private void testButton_Click(object sender, RoutedEventArgs e)
        {
            var batchTranslator = new MarianBatchTranslator(
                this.model.InstallDir,
                this.model.SourceLanguages.Single(),
                this.model.TargetLanguages.Single(),
                this.model.ModelSegmentationMethod,
                false,
                false);


            var sourceLines = File.ReadAllLines(this.SourceFileBox.Text);
            var translationFile = new FileInfo(this.TargetFileBox.Text);
            var refFile = new FileInfo(this.RefFileBox.Text);
            batchTranslator.OutputReady += (x, y) => EvaluateTranslation(refFile, sourceLines, translationFile);
            batchTranslator.BatchTranslate(sourceLines, translationFile);
        }

        private void EvaluateTranslation(FileInfo refFile, IEnumerable<string> input, FileInfo spOutput)
        {

            Log.Information($"Batch translation process for model {this.model} exited. Evaluating output against reference translation.");
            //Queue<string> inputQueue
            //    = new Queue<string>(input);

            var detokOutput = new FileInfo($"{spOutput.FullName}.detok");

            if (spOutput.Exists)
            {
                using (var reader = spOutput.OpenText())
                using (var writer = detokOutput.CreateText())
                {
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        var nonSpLine = (line.Replace(" ", "")).Replace("▁", " ").Trim();
                        writer.WriteLine(nonSpLine);
                    }
                }
            }

            var evaluationScoreFilePath = detokOutput.FullName + ".score.txt";

            var evalProcess = MarianHelper.StartProcessInBackgroundWithRedirects(
                "Evaluate.bat",
                $"{detokOutput.FullName} {refFile.FullName} {evaluationScoreFilePath}",
                (x, y) => this.DisplayResult(evaluationScoreFilePath));
        }

        private void DisplayResult(string evaluationScoreFilePath)
        {
            Dispatcher.Invoke(() => this.ResultBlock.Text = File.ReadAllText(evaluationScoreFilePath));
        }

        private void FileBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textbox = (TextBox)sender;
            textbox.Text = textbox.Text.Replace("\"", "");
        }
    }
}
