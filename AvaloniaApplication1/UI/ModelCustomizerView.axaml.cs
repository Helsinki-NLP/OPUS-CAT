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
using Avalonia;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components.Forms;
using Avalonia.Data;
using Avalonia.Markup.Xaml.Templates;

namespace OpusCatMtEngine
{
    public partial class ModelCustomizerView : UserControl, INotifyPropertyChanged
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

        

        public string ModelTag
        {
            get => modelTag;
            set
            {
                this.modelTagIsValid = false;
                this.AllFieldsAreValid = false;

                //No spaces in model tags, keeps things simple
                modelTag = value.Replace(' ', '_');
                //Remove non-ascii characters (issue with paths)
                modelTag = String.Join("", modelTag.Where(x => x < 127));
                
                if (modelTag == null || modelTag == "")
                {
                    throw new DataValidationException(Properties.Resources.Finetune_ModelTagNotSpecifiedMessage);
                }
                else if (modelTag.Length > OpusCatMtEngineSettings.Default.ModelTagMaxLength)
                {
                    throw new DataValidationException(Properties.Resources.Finetune_ModelTagTooLongMessage);
                }
                else
                {
                    try
                    {
                        var customDir = new DirectoryInfo($"{this.SelectedModel.InstallDir}_{modelTag}");
                        if (customDir.Exists)
                        {
                            throw new DataValidationException(Properties.Resources.Finetune_ModelTagInUseMessage);
                        }

                    }
                    catch (Exception ex)
                    {
                        throw new DataValidationException(Properties.Resources.Finetune_GenericValidationErrorMessage);
                    }
                }

                //TODO: set hacky validation bool, because can't use the same validation
                //system as in WPF
                this.modelTagIsValid = true;
                this.ReValidate();
                NotifyPropertyChanged();
            }
        }

        private string modelTag;

        public string SourceFile 
        { 
            get => sourceFile; 
            set 
            {
                sourceFileIsValid = false;
                this.AllFieldsAreValid = false;

                sourceFile = value.Trim('"');
                var validationMessage = this.ValidateFilePath(sourceFile);
                if (!String.IsNullOrEmpty(validationMessage))
                {
                    throw new DataValidationException(validationMessage);
                }

                sourceFileIsValid = true;
                this.ReValidate();
                NotifyPropertyChanged(); 
            } 
        }

        private string sourceFile;

        public string ValidSourceFile {
            get => validSourceFile;
            set
            {
                validSourceFileIsValid = false;
                this.AllFieldsAreValid = false;

                validSourceFile = value.Trim('"');
                var validationMessage = this.ValidateFilePath(validSourceFile);
                if (!String.IsNullOrEmpty(validationMessage))
                {
                    throw new DataValidationException(validationMessage);
                }

                validSourceFileIsValid = true;
                this.ReValidate();
                NotifyPropertyChanged();
            }
        }

        private string validSourceFile;

        public string TargetFile
        {
            get => targetFile;
            set
            {
                targetFileIsValid = false;
                this.AllFieldsAreValid = false;

                targetFile = value.Trim('"');
                var validationMessage = this.ValidateFilePath(targetFile);
                if (!String.IsNullOrEmpty(validationMessage))
                {
                    throw new DataValidationException(validationMessage);
                }

                targetFileIsValid = true;
                this.ReValidate();
                NotifyPropertyChanged();
            }
        }

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

        public string TmxFile
        {
            get => tmxFile;
            set
            {
                tmxIsValid = false;
                this.AllFieldsAreValid = false;

                tmxFile = value.Trim('"');
                var validationMessage = this.ValidateFilePath(tmxFile);
                if (!String.IsNullOrEmpty(validationMessage))
                {
                    throw new DataValidationException(validationMessage);
                }

                tmxIsValid = true;
                this.ReValidate();
                NotifyPropertyChanged();
            }
        }

        private string tmxFile;

        public string ValidTargetFile
        {
            get => validTargetFile;
            set
            {
                validTargetFileIsValid = false;
                this.AllFieldsAreValid = false;

                validTargetFile = value.Trim('"');
                var validationMessage = this.ValidateFilePath(validTargetFile);
                if (!String.IsNullOrEmpty(validationMessage))
                {
                    throw new DataValidationException(validationMessage);
                }

                validTargetFileIsValid = true;
                this.ReValidate();
                NotifyPropertyChanged();
            }
        }

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

        public bool ModelTagIsValid
        {
            get => modelTagIsValid;
            set
            {
                modelTagIsValid = value;
                NotifyPropertyChanged();
            }
        }

        private bool modelTagIsValid;

        public bool AllFieldsAreValid
        { 
            get => allFieldsAreValid;
            set
            {
                allFieldsAreValid = value;
                NotifyPropertyChanged();
            }
        }

        public bool TmxIsValid
        {
            get => tmxIsValid;
            set
            {
                tmxIsValid = value;
                NotifyPropertyChanged();
            }
        }

        private bool tmxIsValid;

        public bool SourceFileIsValid
        {
            get => sourceFileIsValid;
            set
            {
                sourceFileIsValid = value;
                NotifyPropertyChanged();
            }
        }

        private bool sourceFileIsValid;

        public bool TargetFileIsValid
        {
            get => targetFileIsValid;
            set
            {
                targetFileIsValid = value;
                NotifyPropertyChanged();
            }
        }

        private bool targetFileIsValid;


        public bool ValidTargetFileIsValid
        {
            get => validTargetFileIsValid;
            set
            {
                validTargetFileIsValid = value;
                NotifyPropertyChanged();
            }
        }

        private bool validTargetFileIsValid;

        public bool ValidSourceFileIsValid
        {
            get => validSourceFileIsValid;
            set
            {
                validSourceFileIsValid = value;
                NotifyPropertyChanged();
            }
        }

        private bool validSourceFileIsValid;


        private string validTargetFile;


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

            this.TmxFiles.IsChecked = true;
            this.SplitValidationFiles.IsChecked = true;
        }

        private async void customize_Click(object sender, RoutedEventArgs e)
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

                        await box.ShowAsync();

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
                            await box.ShowAsync();
                            return;
                            
                        }
                    }
                    break;
                default:
                    break;
            }


            if (this.validationFileType == ValidationFileType.Separate)
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
        private bool allFieldsAreValid;

        private void ValidationFileTypeButton_Checked(object sender, RoutedEventArgs e)
        {
            var radioButton = ((RadioButton)sender);
            if (radioButton.IsChecked.Value)
            {
                switch (radioButton.Name)
                {
                    case "SeparateValidationFiles":
                        this.validationFileType = ValidationFileType.Separate;
                        break;
                    case "SplitValidationFiles":
                        this.validationFileType = ValidationFileType.Split;
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

        private void ReValidate()
        {
            this.AllFieldsAreValid = true;
            if (this.inputType == InputFileType.TxtFile)
            {
                if (!this.sourceFileIsValid || !this.targetFileIsValid)
                {
                    this.AllFieldsAreValid = false;
                    return;
                }
            }
            else if (this.inputType == InputFileType.TmxFile)
            {
                if (!this.tmxIsValid)
                {
                    this.AllFieldsAreValid = false;
                    return;
                }
            }

            if (this.validationFileType == ValidationFileType.Separate)
            {
                if (!this.validSourceFileIsValid || !this.validTargetFileIsValid)
                {
                    this.AllFieldsAreValid = false;
                    return;
                }
            }

            if (!this.modelTagIsValid)
            {
                this.AllFieldsAreValid = false;
                return;
            }
        }
    }

}
