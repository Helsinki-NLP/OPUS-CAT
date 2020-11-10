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


namespace FiskmoMTEngine
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
                modelTag = value;
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
                        validationMessage = "Model tag not specified.";
                    }
                    else if (this.ModelTag.Length > FiskmoMTEngineSettings.Default.ModelTagMaxLength)
                    {
                        validationMessage = "Model tag is too long.";
                    }
                    else
                    {
                        try
                        {
                            var customDir = new DirectoryInfo($"{this.selectedModel.InstallDir}_{this.ModelTag}");
                            if (customDir.Exists)
                            {
                                validationMessage = "Model tag is already in use for this base model";
                            }

                        }
                        catch (Exception ex)
                        {
                            validationMessage = "Error";
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
                validationMessage = "File does not exist.";
            }
            return validationMessage;
        }

        public ModelCustomizerView(MTModel selectedModel)
        {
            this.CustomizationNotStarted = true;
            this.selectedModel = selectedModel;
            
            this.Title = $"Customize model {this.selectedModel.Name}";
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
                    filePair = TmxToTxtParser.ParseTmxToParallelFiles(
                        this.TmxFile,
                        this.selectedModel.SourceLanguages.Single(),
                        this.selectedModel.TargetLanguages.Single(),
                        this.IncludePlaceholderTagsBox.IsChecked.Value,
                        this.IncludeTagPairBox.IsChecked.Value
                        );
                    break;
                default:
                    break;
            }
            var customDir = new DirectoryInfo($"{this.selectedModel.InstallDir}_{this.ModelTag}");

            if (this.SeparateValidationFiles.IsChecked.Value)
            {
                validPair = new ParallelFilePair(this.ValidSourceFileBox.Text, this.ValidTargetFileBox.Text);
            }
            else
            {
                (filePair, validPair) = HelperFunctions.SplitFilePair(filePair, FiskmoMTEngineSettings.Default.IDValidSetSize);
            }

            var modelManager = ((ModelManager)this.DataContext);

            modelManager.StartCustomization(
                filePair,
                validPair,
                null,
                this.selectedModel.SourceLanguages.Single(),
                this.selectedModel.TargetLanguages.Single(),
                this.ModelTag,
                this.IncludePlaceholderTagsBox.IsChecked.Value,
                this.IncludeTagPairBox.IsChecked.Value,
                customDir,
                this.selectedModel);

            this.CustomizationNotStarted = false;

        }
        private void browse_Click(object sender, RoutedEventArgs e)
        {
            var pathBox = (TextBox)((Button)sender).DataContext;

            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

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