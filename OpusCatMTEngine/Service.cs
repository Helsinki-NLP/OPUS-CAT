using System;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Web;
using OpusMTInterface;
using Serilog;

namespace OpusCatMTEngine
{

    class Service
    {
        private ServiceHost StartNetTcpAndHttpService(ModelManager modelManager, Boolean onlyNetTcp)
        {
            Uri[] baseAddresses;
            if (onlyNetTcp)
            {
                baseAddresses = new Uri[] {
                    new Uri($"net.tcp://localhost:{OpusCatMTEngineSettings.Default.MtServicePort}/")
                };
            }
            else
            {
                baseAddresses = new Uri[] {
                    new Uri($"net.tcp://localhost:{OpusCatMTEngineSettings.Default.MtServicePort}/"),
                    new Uri($"http://localhost:8500/") };
            };

            var mtService = new MTService(modelManager);

            Log.Information($"Creating service host with following URIs: {String.Join(",",baseAddresses.Select(x => x.ToString()))}");

            var selfHost = new ServiceHost(mtService, baseAddresses);
            
            var nettcpBinding = new NetTcpBinding();
            
            //Use default net.tcp security, which is based on Windows authentication:
            //using the service is only possible from other computers in the same domain.
            //TODO: add a checkbox (with warning) in the UI for using security mode None,
            //to allow connections from IP range (also add same checkbox to clients). 

            //nettcpBinding.Security.Mode = SecurityMode.None;
            /*nettcpBinding.Security.Mode = SecurityMode.Transport;
            nettcpBinding.Security.Transport.ClientCredentialType =
                TcpClientCredentialType.Windows;*/

            //Customization tuning sets tend to be big
            nettcpBinding.MaxReceivedMessageSize = 20000000;
            
            selfHost.AddServiceEndpoint(typeof(IMTService), nettcpBinding, "MTService");

            if (!onlyNetTcp)
            {
                selfHost.AddServiceEndpoint(typeof(IMTService), new WebHttpBinding(), "MTRestService");
                WebHttpBehavior helpBehavior = new WebHttpBehavior();
                helpBehavior.HelpEnabled = true;
                selfHost.Description.Endpoints[1].Behaviors.Add(helpBehavior);
                
            }

            /*
            // Check to see if the service host already has a ServiceMetadataBehavior
            ServiceMetadataBehavior smb = selfHost.Description.Behaviors.Find<ServiceMetadataBehavior>();
            // If not, add one
            if (smb == null)
                smb = new ServiceMetadataBehavior();
            //smb.HttpGetEnabled = true;
            smb.MetadataExporter.PolicyVersion = PolicyVersion.Policy15;
            selfHost.Description.Behaviors.Add(smb);

            // Add MEX endpoint
            selfHost.AddServiceEndpoint(
                ServiceMetadataBehavior.MexContractName,
                MetadataExchangeBindings.CreateMexTcpBinding(),
                "mex"
            );
            */

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
            if (OpusCatMTEngineSettings.Default.StartHttpService)
            {
                Log.Information("Starting OPUS-CAT MT Engine's net.tcp and HTTP APIs");
                try
                {
                    host = this.StartNetTcpAndHttpService(modelManager, false);
                    Log.Information("net.tcp and HTTP APIs were started");
                }
                catch (System.ServiceModel.AddressAccessDeniedException ex)
                {
                    Log.Information("HTTP API could not be started, starting Net.tcp API. If HTTP API is required, add the relevant URL to the urlacl list with netsh.");
                    host = this.StartNetTcpAndHttpService(modelManager, true);
                }
            }
            else
            {
                Log.Information("Starting Net.tcp API only, HTTP API can be enabled in the settings.");
                host = this.StartNetTcpAndHttpService(modelManager, true);
            }

            return host;
        }
    }
}
