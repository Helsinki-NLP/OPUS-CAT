using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Avalonia.Platform.Storage;
using Octokit;
using Avalonia.Controls.Shapes;
using Avalonia;

namespace OpusCatMtEngine
{
    public partial class ModelCustomizerView : UserControl, IDataErrorInfo, INotifyPropertyChanged
    {
        private MTModel selectedModel;

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


        public string ModelTag
        {
            get => modelTag;
            set
            {
                //No spaces in model tags, keeps things simple
                modelTag = value.Replace(' ', '_');
                //Remove non-ascii characters (issue with paths)
                modelTag = String.Join("", modelTag.Where(x => x < 127));
                NotifyPropertyChanged();
            }
        }

        private string modelTag;

        public string SourceFile { get => sourceFile; set { sourceFile = value.Trim('"'); NotifyPropertyChanged(); } }

        private string sourceFile;

        public string ValidSourceFile { get => validSourceFile; set { validSourceFile = value.Trim('"'); NotifyPropertyChanged(); } }

        private string validSourceFile;

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

        public string ValidTargetFile { get => validTargetFile; set { validTargetFile = value.Trim('"'); NotifyPropertyChanged(); } }

        public bool CustomizationNotStarted
        {
            get => _customizationNotStarted;
            set
            {
                _customizationNotStarted = value;
                NotifyPropertyChanged();
            }
        }

        public MTModel SelectedModel
        {
            get => selectedModel;
            set
            {
                selectedModel = value;
                NotifyPropertyChanged();
            }
        }

        private string validTargetFile;


        private string Validate(string propertyName)
        {
            // Return error message if there is error on else return empty or null string
            string validationMessage = string.Empty;

            switch (propertyName)
            {
                case "ModelTag":

                    if (this.ModelTag == null || this.ModelTag == "")
                    {
                        validationMessage = Properties.Resources.Finetune_ModelTagNotSpecifiedMessage;
                    }
                    else if (this.ModelTag.Length > OpusCatMtEngineSettings.Default.ModelTagMaxLength)
                    {
                        validationMessage = Properties.Resources.Finetune_ModelTagTooLongMessage;
                    }
                    else
                    {
                        try
                        {
                            var customDir = new DirectoryInfo($"{this.SelectedModel.InstallDir}_{this.ModelTag}");
                            if (customDir.Exists)
                            {
                                validationMessage = Properties.Resources.Finetune_ModelTagInUseMessage;
                            }

                        }
                        catch (Exception ex)
                        {
                            validationMessage = Properties.Resources.Finetune_GenericValidationErrorMessage;
                        }
                    }

                    break;
                case "ValidSourceFile":
                    validationMessage = ValidateFilePath(this.ValidSourceFile);
                    break;
                case "ValidTargetFile":
                    validationMessage = ValidateFilePath(this.ValidTargetFile);
                    break;
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
                validationMessage = Properties.Resources.Finetune_FileDoesNotExistMessage;
            }
            return validationMessage;
        }

        public ModelCustomizerView()
        {

        }

        public ModelCustomizerView(MTModel selectedModel)
        {
            this.CustomizationNotStarted = true;
            this.SelectedModel = selectedModel;
            this.SourceLanguage = selectedModel.SourceLanguages.First();
            this.TargetLanguage = selectedModel.TargetLanguages.First();

            this.Title = String.Format(Properties.Resources.Finetune_FineTuneWindowTitle, this.SelectedModel.Name);
            InitializeComponent();
        }

        private void customize_Click(object sender, RoutedEventArgs e)
        {

            ParallelFilePair filePair = null;
            ParallelFilePair validPair = null;

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
                            this.IncludePlaceholderTagsBox.IsChecked.Value,
                            this.IncludeTagPairBox.IsChecked.Value
                            );
                    if (filePair == null)
                    {
                        var box = MessageBoxManager.GetMessageBoxStandard(
                                "Tmx file not valid",
                                String.Format(
                                    Properties.Resources.Finetune_TmxFileNotValidMessage, this.TmxFile),
                                ButtonEnum.Ok);
                        return;
                        
                    }

                    if (filePair.SentenceCount < OpusCatMtEngineSettings.Default.FinetuningSetMinSize)
                    {
                        var eligibleLangPairs =
                            tmxParser.TmxLangCounts.Where(x => x.Value > OpusCatMtEngineSettings.Default.FinetuningSetMinSize);
                        if (eligibleLangPairs.Count() > 0)
                        {
                            var selectionWindow = new SelectTmxLangPairWindow(eligibleLangPairs);
                            if (Avalonia.Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                            {
                                var dialogResult = selectionWindow.ShowDialog(desktop.MainWindow);
                                dialogResult.Wait();
                                if (dialogResult.IsCompleted)
                                {
                                    filePair = tmxParser.ParseTmxToParallelFiles(
                                        this.TmxFile,
                                        new IsoLanguage(selectionWindow.SelectedPair.Key.Item1),
                                        new IsoLanguage(selectionWindow.SelectedPair.Key.Item2),
                                        this.IncludePlaceholderTagsBox.IsChecked.Value,
                                        this.IncludeTagPairBox.IsChecked.Value
                                    );
                                }
                                else
                                {
                                    return;
                                }
                            }
                        }
                        else
                        {
                            var box = MessageBoxManager.GetMessageBoxStandard(
                                "Not enough segments",
                                Properties.Resources.Finetune_NotEnoughSegmentsInTmx,
                                ButtonEnum.Ok);
                            return;
                        }
                    }
                    break;
                default:
                    break;
            }


            if (this.SeparateValidationFiles.IsChecked.Value)
            {
                validPair = new ParallelFilePair(this.ValidSourceFileBox.Text, this.ValidTargetFileBox.Text);
            }
            else
            {
                (filePair, validPair) = HelperFunctions.SplitFilePair(filePair, OpusCatMtEngineSettings.Default.IDValidSetSize);
            }

            var modelManager = ((ModelManager)this.DataContext);

            modelManager.StartCustomization(
                filePair,
                validPair,
                null,
                this.SourceLanguage,
                this.TargetLanguage,
                this.ModelTag,
                this.IncludePlaceholderTagsBox.IsChecked.Value,
                this.IncludeTagPairBox.IsChecked.Value,
                this.SelectedModel);

            this.CustomizationNotStarted = false;
        }

        private async void browse_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var pathBox = (TextBox)button.DataContext;

            var topLevel = TopLevel.GetTopLevel(this);

            FilePickerFileType filter;
            switch((string)button.Tag)
            {
                case "TMX Files|*.tmx":
                    filter = HelperFunctions.TmxFilePickerType;
                    break;
                default:
                    filter = FilePickerFileTypes.All;
                    break;
            }

            // Start async operation to open the dialog.
            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Select file",
                AllowMultiple = false,
                FileTypeFilter = new[] { filter }

            });

            if (files.Count >= 1)
            {
                //TODO: Fix the path handling here, avalonia adds file:/// to path
                pathBox.Text = new FileInfo(files.First().Path.AbsolutePath).FullName;
            }
        }

        enum InputFileType { TxtFile, TmxFile };
        private InputFileType inputType;

        enum ValidationFileType { Split, Separate };
        private ValidationFileType validationFileType;
        private bool _customizationNotStarted;

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

        //TODO: rewire the validation events, IsEnabledChanged does not exist
        private void ReValidate(object sender, AvaloniaPropertyChangedEventArgs e)
        {
            NotifyPropertyChanged("ValidSourceFile");
            NotifyPropertyChanged("TmxFile");
            NotifyPropertyChanged("ValidTargetFile");
            NotifyPropertyChanged("SourceFile");
            NotifyPropertyChanged("TargetFile");
        }
    }

}
