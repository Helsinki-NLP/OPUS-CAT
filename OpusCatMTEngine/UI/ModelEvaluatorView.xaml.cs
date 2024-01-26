using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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
using Path = System.IO.Path;

namespace OpusCatMtEngine
{
    public partial class ModelEvaluatorView : UserControl, IDataErrorInfo, INotifyPropertyChanged
    {
        private List<MTModel> selectedModels;

        public string Title { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public string this[string columnName]
        {
            get
            {
                return Validate(columnName);
            }
        }

        public string Error
        {
            get { return "...."; }
        }
        
        

        public string SourceFile { get => sourceFile; set { sourceFile = value.Trim('"'); NotifyPropertyChanged(); } }

        private string sourceFile;


        public string TargetFile { get => targetFile; set { targetFile = value.Trim('"'); NotifyPropertyChanged(); } }

        private string targetFile;

        public IsoLanguage TargetLanguage
        {
            get => targetLanguage;
            set { targetLanguage = value; NotifyPropertyChanged(); }
        }

        public IsoLanguage SourceLanguage
        {
            get => sourceLanguage;
            set { sourceLanguage = value; NotifyPropertyChanged(); }
        }

        private IsoLanguage sourceLanguage;

        private IsoLanguage targetLanguage;

        public string TmxFile { get => tmxFile; set { tmxFile = value.Trim('"'); NotifyPropertyChanged(); } }

        private string tmxFile;

        public bool EvaluationNotStarted
        {
            get => _evaluationNotStarted;
            set
            {
                _evaluationNotStarted = value;
                NotifyPropertyChanged();
            }
        }

        public List<IsoLanguage> CommonSourceLanguages
        {
            get
            {
                return SelectedModels.Select(x => x.SourceLanguages).Aggregate((x,y) => x.Intersect(y).ToList());
            }
        }

        public List<IsoLanguage> CommonTargetLanguages
        {
            get
            {
                return SelectedModels.Select(x => x.TargetLanguages).Aggregate((x, y) => x.Intersect(y).ToList());
            }
        }

        public List<MTModel> SelectedModels
        {
            get => selectedModels;
            set
            {
                selectedModels = value;
                NotifyPropertyChanged();
            }
        }
        

        private string Validate(string propertyName)
        {
            // Return error message if there is error on else return empty or null string
            string validationMessage = string.Empty;

            switch (propertyName)
            {
                case "SourceFile":
                    validationMessage = ValidateFilePath(this.SourceFile);
                    break;
                case "TargetFile":
                    validationMessage = ValidateFilePath(this.TargetFile);
                    break;
                case "TmxFile":
                    validationMessage = ValidateFilePath(this.TmxFile);
                    break;
            }

            return validationMessage;
        }

        private string ValidateFilePath(string filePath)
        {
            string validationMessage = String.Empty;
            if (filePath == null || !File.Exists(filePath))
            {
                validationMessage = OpusCatMtEngine.Properties.Resources.Finetune_FileDoesNotExistMessage;
            }
            return validationMessage;
        }

        public ModelEvaluatorView(IEnumerable<MTModel> selectedModels)
        {
            this.EvaluationNotStarted = true;
            this.SelectedModels = selectedModels.ToList();
            this.SourceLanguage = CommonSourceLanguages.First();
            this.TargetLanguage = CommonTargetLanguages.First();
            
            this.Title = String.Format(OpusCatMtEngine.Properties.Resources.Finetune_FineTuneWindowTitle,
                this.SelectedModels.Select(x => x.Name).Aggregate((x,y) => $"{x}, {y}"));
            InitializeComponent();
        }

        private void evaluate_Click(object sender, RoutedEventArgs e)
        {

            var evalDirPath = HelperFunctions.GetOpusCatDataPath(OpusCatMTEngineSettings.Default.EvaluationDir);
            var evalDir = new DirectoryInfo(evalDirPath);
            if (!evalDir.Exists)
            {
                evalDir.Create();
            }

            var evalRunDir = evalDir.CreateSubdirectory(DateTime.Now.ToString("yyyy-dd-M--HH-mm--ss"));

            ParallelFilePair filePair = null;

            switch (this.inputType)
            {
                case InputFileType.TxtFile:
                    filePair = new ParallelFilePair(this.SourceFile, this.TargetFile);
                    break;
                case InputFileType.TmxFile:
                    var tmxParser = new TmxToTxtParser();
                    filePair = tmxParser.ParseTmxToParallelFiles(
                            this.TmxFile,
                            this.SourceLanguage,
                            this.TargetLanguage,
                            false,
                            false
                            );
                    if (filePair == null)
                    {
                        MessageBox.Show(
                            String.Format(
                                OpusCatMtEngine.Properties.Resources.Finetune_TmxFileNotValidMessage,this.TmxFile));
                        return;
                    }

                    if (filePair.SentenceCount == 0)
                    {
                        var eligibleLangPairs = 
                            tmxParser.TmxLangCounts.Where(x => x.Value > OpusCatMTEngineSettings.Default.FinetuningSetMinSize);
                        if (eligibleLangPairs.Count() > 0)
                        {
                            var selectionWindow = new SelectTmxLangPairWindow(eligibleLangPairs);
                            var dialogResult = selectionWindow.ShowDialog();
                            if (dialogResult.HasValue && dialogResult.Value)
                            {
                                filePair = tmxParser.ParseTmxToParallelFiles(
                                    this.TmxFile,
                                    new IsoLanguage(selectionWindow.SelectedPair.Key.Item1),
                                    new IsoLanguage(selectionWindow.SelectedPair.Key.Item2),
                                    false,
                                    false
                                );
                            }
                            else
                            {
                                return;
                            }
                        }
                        else
                        {
                            MessageBox.Show(
                                String.Format(
                                    OpusCatMtEngine.Properties.Resources.Finetune_NotEnoughSegmentsInTmx));
                            return;
                        }
                    }
                    break;
                default:
                    break;
            }
            
            foreach (var model in this.SelectedModels)
            {
                var targetFile = new FileInfo(Path.Combine(evalRunDir.FullName,$"{model.Name}.txt"));
                model.TranslateAndEvaluate(
                    filePair.Source,
                    targetFile,
                    filePair.Target,
                    0, this.sourceLanguage, this.targetLanguage);
            }
        }

        private void browse_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var pathBox = (TextBox)button.DataContext;
            
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Filter = (string)button.Tag;
            // Show open file dialog box
            bool? result = dlg.ShowDialog();

            if (result == true)
            {
                pathBox.Text = dlg.FileName;
            }
        }

        enum InputFileType { TxtFile, TmxFile };
        private InputFileType inputType;

        enum ValidationFileType { Split, Separate };
        private ValidationFileType validationFileType;
        private bool _evaluationNotStarted;

        private void ValidationFileTypeButton_Checked(object sender, RoutedEventArgs e)
        {
            var radioButton = ((RadioButton)sender);
            if (radioButton.IsChecked.Value)
            {
                switch (radioButton.Name)
                {
                    case "SeparateValidationFiles":
                        this.validationFileType = ValidationFileType.Split;
                        break;
                    case "SplitValidationFiles":
                        this.validationFileType = ValidationFileType.Separate;
                        break;
                }
            }

        }

        private void ModeButton_Checked(object sender, RoutedEventArgs e)
        {
            var radioButton = ((RadioButton)sender);
            if (radioButton.IsChecked.Value)
            {
                switch (radioButton.Name)
                {
                    case "TxtFiles":
                        this.inputType = InputFileType.TxtFile;
                        break;
                    case "TmxFiles":
                        this.inputType = InputFileType.TmxFile;
                        break;
                }

            }
        }

        private void ReValidate(object sender, DependencyPropertyChangedEventArgs e)
        {
            NotifyPropertyChanged("ValidSourceFile");
            NotifyPropertyChanged("TmxFile");
            NotifyPropertyChanged("ValidTargetFile");
            NotifyPropertyChanged("SourceFile");
            NotifyPropertyChanged("TargetFile");
        }
    }
}