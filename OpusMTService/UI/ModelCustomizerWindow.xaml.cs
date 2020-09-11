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
    public partial class ModelCustomizerWindow : Window, IDataErrorInfo, INotifyPropertyChanged
    {
        private MTModel selectedModel;

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
            }

            return validationMessage;
        }


        public ModelCustomizerWindow(MTModel selectedModel)
        {
            this.selectedModel = selectedModel;
            this.Title = $"Customize model {this.selectedModel.Name}";
            InitializeComponent();
        }
        

        private void CloseCommandHandler(object sender, ExecutedRoutedEventArgs e)
        {
            this.Close();
        }

        private void customize_Click(object sender, RoutedEventArgs e)
        {
            ParallelFilePair filePair = null;
            switch (this.inputType)
            {
                case InputFileType.TxtFile:
                    filePair = new ParallelFilePair(this.SourceFileBox.Text, this.TargetFileBox.Text); 
                    break;
                case InputFileType.TmxFile:
                    filePair = TmxToTxtParser.ParseTmxToParallelFiles(
                        this.SourceFileBox.Text, this.selectedModel.SourceLanguages.Single(), this.selectedModel.TargetLanguages.Single());
                    break;
                default:
                    break;
            }
            var customDir = new DirectoryInfo($"{this.selectedModel.InstallDir}_{this.ModelTag}");

            var customizer = new MarianCustomizer(
                this.selectedModel,
                filePair.Source,
                filePair.Target,
                new FileInfo(this.ValidSourceFileBox.Text),
                new FileInfo(this.ValidTargetFileBox.Text),
                this.LabelBox.Text,
                false,
                false,
                customDir
                );
            customizer.Customize(null);
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

        enum InputFileType { TxtFile, TmxFile};
        private InputFileType inputType;

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
    }
}
