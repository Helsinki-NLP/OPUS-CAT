using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    public partial class ModelCustomizerWindow : Window
    {
        private MTModel selectedModel;

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
            /*var customizer = new MarianCustomizer(
                this.selectedModel,
                new FileInfo(this.SourceFileBox.Text),
                new FileInfo(this.TargetFileBox.Text),
                new FileInfo(this.ValidSourceFileBox.Text),
                new FileInfo(this.ValidTargetFileBox.Text),
                this.LabelBox.Text,
                false,
                false,

                );
            customizer.Customize(null);*/
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
    }
}
