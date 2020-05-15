using System;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Web;
using OpusMTInterface;
using Serilog;

namespace FiskmoMTEngine
{

    class Service
    {
        private ServiceHost StartNetTcpAndHttpService(ModelManager modelManager, Boolean onlyNetTcp)
        {
            Uri[] baseAddresses;
            if (onlyNetTcp)
            {
                baseAddresses = new Uri[] {
                    new Uri($"net.tcp://localhost:{FiskmoMTEngineSettings.Default.MtServicePort}/")
                };
            }
            else
            {
                baseAddresses = new Uri[] {
                    new Uri($"net.tcp://localhost:{FiskmoMTEngineSettings.Default.MtServicePort}/"),
                    new Uri($"http://localhost:8500/") };
            };

            var mtService = new MTService(modelManager);

            Log.Information($"Creating service host with following URIs: {String.Join(",",baseAddresses.Select(x => x.ToString()))}");

            var selfHost = new ServiceHost(mtService, baseAddresses);

            // Check to see if the service host already has a ServiceMetadataBehavior
            ServiceMetadataBehavior smb = selfHost.Description.Behaviors.Find<ServiceMetadataBehavior>();
            // If not, add one
            if (smb == null)
                smb = new ServiceMetadataBehavior();
            smb.MetadataExporter.PolicyVersion = PolicyVersion.Policy15;
            selfHost.Description.Behaviors.Add(smb);
            // Add MEX endpoint
            selfHost.AddServiceEndpoint(
                ServiceMetadataBehavior.MexContractName,
                MetadataExchangeBindings.CreateMexTcpBinding(),
                "mex"
            );

            var nettcpBinding = new NetTcpBinding();
            nettcpBinding.Security.Mode = SecurityMode.Transport;
            nettcpBinding.Security.Transport.ClientCredentialType =
                TcpClientCredentialType.Windows;

            //Customization tuning sets tend to be big
            nettcpBinding.MaxReceivedMessageSize = 20000000;
            selfHost.AddServiceEndpoint(typeof(IMTService), nettcpBinding, "MTService");

            if (!onlyNetTcp)
            {
                selfHost.AddServiceEndpoint(typeof(IMTService), new WebHttpBinding(), "MTRestService");
                WebHttpBehavior helpBehavior = new WebHttpBehavior();
                helpBehavior.HelpEnabled = true;
                selfHost.Description.Endpoints[2].Behaviors.Add(helpBehavior);
            }
            
            Log.Information($"Opening the service host");
            selfHost.Open();
            return selfHost;
        }



        public ServiceHost StartService(ModelManager modelManager)
        {
            ServiceHost host;

            //Launching the http service requires that the program is run with
            //administrator privileges or that the port has been enabled for http 
            //with netsh http add urlacl

            Log.Information("Starting Fiskmö MT service's net.tcp and HTTP APIs");
            try
            {
                host = this.StartNetTcpAndHttpService(modelManager,false);
                Log.Information("net.tcp and HTTP APIs were started");
            }
            catch (System.ServiceModel.AddressAccessDeniedException ex)
            {
                Log.Information("HTTP API could not be started, starting Net.tcp API. If HTTP API is required, add the relevant URL to the urlacl list with netsh.");
                host = this.StartNetTcpAndHttpService(modelManager, true);
            }

            return host;
        }
    }
}
