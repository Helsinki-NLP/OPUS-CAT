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
            this.Model = selectedModel;
            this.Title = $"Translating with model {Model.Name}";
            InitializeComponent();
        }

        public MTModel Model { get => model; set => model = value; }

        private async void translateButton_Click(object sender, RoutedEventArgs e)
        {
            await Task.Run(() => Dispatcher.Invoke(() =>
            {

                /*byte[] bytes = Encoding.Default.GetBytes(this.SourceBox.Text);
                var utf8Source = Encoding.ASCII.GetString(bytes);*/
                this.TargetBox.Text = this.Model.Translate(this.SourceBox.Text);
            }));
        }
    }
}
