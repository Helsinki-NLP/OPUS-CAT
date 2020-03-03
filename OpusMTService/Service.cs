using System;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Web;
using OpusMTInterface;

namespace OpusMTService
{

    class Service
    {
        public ServiceHost StartService(ModelManager modelManager)
        {
            
            var baseAddress = new Uri("net.tcp://localhost:8477/");

            var mtService = new MTService(modelManager);

            var selfHost = new ServiceHost(mtService, baseAddress);
            /*
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
            */
            selfHost.AddServiceEndpoint(typeof(IMTService), new NetTcpBinding(), "MTService");
            selfHost.Open();

            return selfHost;
        }
    }
}
