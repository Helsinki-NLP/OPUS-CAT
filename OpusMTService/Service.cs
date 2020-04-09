using System;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Web;
using OpusMTInterface;
using Serilog;

namespace OpusMTService
{

    class Service
    {
        private ServiceHost StartNetTcpAndHttpService(ModelManager modelManager)
        {
            Uri[] baseAddresses = {
                new Uri($"net.tcp://localhost:{OpusMTServiceSettings.Default.MtServicePort}/"),
                new Uri($"http://localhost:8500/") };

            var mtService = new MTService(modelManager);

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


            selfHost.AddServiceEndpoint(typeof(IMTService), new NetTcpBinding(), "MTService");
            selfHost.AddServiceEndpoint(typeof(IMTService), new WebHttpBinding(), "MTRestService");

            WebHttpBehavior helpBehavior = new WebHttpBehavior();
            helpBehavior.HelpEnabled = true;
            selfHost.Description.Endpoints[2].Behaviors.Add(helpBehavior);
            selfHost.Open();

            return selfHost;
        }

        private ServiceHost StartNetTcpServiceOnly(ModelManager modelManager)
        {
            Uri[] baseAddresses = {
                new Uri($"net.tcp://localhost:{OpusMTServiceSettings.Default.MtServicePort}/")
            };

            var mtService = new MTService(modelManager);

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


            selfHost.AddServiceEndpoint(typeof(IMTService), new NetTcpBinding(), "MTService");
            
            selfHost.Open();

            return selfHost;
        }


        public ServiceHost StartService(ModelManager modelManager)
        {
            ServiceHost host;

            //Launching the http service requires that the program is run with
            //administrator privileges or that the port has been enabled for http 
            //with 
            try
            {
                host = this.StartNetTcpAndHttpService(modelManager);
            }
            catch (System.ServiceModel.AddressAccessDeniedException ex)
            {
                Log.Information("HTTP API could not be started, starting Net.tcp API");
                host = this.StartNetTcpServiceOnly(modelManager);
            }

            return host;
        }
    }
}
