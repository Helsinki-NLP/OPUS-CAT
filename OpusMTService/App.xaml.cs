using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows;

namespace OpusMTService
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private ServiceHost serviceHost;

        public ModelManager ModelManager { get; private set; }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // Create the startup window
            MainWindow wnd = new MainWindow();
            //wnd.DataContext = this.ModelManager;
            // Do stuff here, e.g. to the window
            // Show the window
            wnd.Show();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            /*var service = new Service();
            this.ModelManager = new ModelManager();
            this.serviceHost = service.StartService(this.ModelManager);*/
            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
        }
    }
}
