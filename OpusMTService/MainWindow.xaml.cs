using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OpusMTService
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ModelManager ModelManager { get; private set; }

        private ServiceHost serviceHost;

        public MainWindow()
        {
            this.StartService();
            InitializeComponent();
            this.ServicePortBox.Text = OpusMTServiceSettings.Default.MtServicePort;
        }

        private void StartService()
        {
            var service = new Service();
            this.ModelManager = new ModelManager();
            this.DataContext = this.ModelManager;
            this.serviceHost = service.StartService(this.ModelManager);
        }
        
        private void restartButton_Click(object sender, RoutedEventArgs e)
        {
            //Close service and start it again
            this.serviceHost.Close();
            this.StartService();
        }

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            OpusMTServiceSettings.Default.MtServicePort = this.ServicePortBox.Text;
            OpusMTServiceSettings.Default.Save();
        }

        private void ServicePortBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.serviceHost.Close();
        }
    }
}
