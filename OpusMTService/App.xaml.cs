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
        
        protected override void OnStartup(StartupEventArgs e)
        {
            var service = new Service();
            this.ModelManager = MTService.ModelManager;
            this.serviceHost = service.StartService();
            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            this.serviceHost.Close();
            base.OnExit(e);
        }
    }
}
