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

namespace OpusMTService
{
    /// <summary>
    /// Interaction logic for TranslateWindow.xaml
    /// </summary>
    public partial class TranslateWindow : Window
    {
        
        private MTModel model;

        public TranslateWindow(MTModel selectedModel)
        {
            this.model = selectedModel;
            InitializeComponent();
        }
        
        private async void translateButton_Click(object sender, RoutedEventArgs e)
        {
            await Task.Run(()=>Dispatcher.Invoke(()=> this.TargetBox.Text = this.model.Translate(this.SourceBox.Text)));
        }
    }
}
