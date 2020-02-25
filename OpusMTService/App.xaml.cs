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
        public MarianManager MarianManager { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            this.ModelManager = new ModelManager();
            this.MarianManager = new MarianManager(this.ModelManager);
            var service = new Service();
            this.serviceHost = service.StartService(this.ModelManager, this.MarianManager);
            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            this.serviceHost.Close();
            base.OnExit(e);
        }
    }
}
