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
            this.Model = selectedModel;
            this.Title = $"Translating with model {Model.Name}";
            InitializeComponent();
        }

        public MTModel Model { get => model; set => model = value; }

        private void translateButton_Click(object sender, RoutedEventArgs e)
        {
            var source = this.SourceBox.Text;
            Task<string> translate = new Task<string>(() => this.Model.Translate(source));
            translate.ContinueWith(x => Dispatcher.Invoke(() => this.TargetBox.Text = x.Result));
            translate.Start();
            
        }
    }
}
