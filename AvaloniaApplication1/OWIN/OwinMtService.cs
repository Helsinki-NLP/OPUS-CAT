
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Serilog;
using Microsoft.AspNetCore.Owin;

using OpusCatMtEngine;
using Microsoft.AspNetCore.Builder;

namespace OpusCatMtEngine
{
    public class OwinMtService
    {
        public OwinMtService(ModelManager modelManager)
        {
            
            string baseAddress;
            if (OpusCatMtEngineSettings.Default.AllowRemoteUse)
            {
                baseAddress = $"http://+:{OpusCatMtEngineSettings.Default.HttpMtServicePort}";
            }
            else
            {                
                baseAddress = $"http://localhost:{OpusCatMtEngineSettings.Default.HttpMtServicePort}";
            }

            //First try to open the external http listener, this requires admin (or a prior
            //reservation of the port with netsh)
            try
            {
                this.StartWebApp($"http://+:{OpusCatMtEngineSettings.Default.HttpMtServicePort}", modelManager);
                Log.Information($"Started HTTP API at http://+:{OpusCatMtEngineSettings.Default.HttpMtServicePort}. This API can be accessed from remote computers, if the firewall has been configured to allow it.");
            }
            //If opening the external listener fails, open a localhost listener (works without admin).
            catch (Exception ex)
            {
                this.StartWebApp($"http://localhost:{OpusCatMtEngineSettings.Default.HttpMtServicePort}", modelManager);
                Log.Information($"Started HTTP API at http://localhost:{OpusCatMtEngineSettings.Default.HttpMtServicePort}. This API cannot be accessed from remote computers.");
            }

        }

        private void StartWebApp(string baseAddress, ModelManager modelManager)
        {
            var builder = WebApplication.CreateBuilder();
            var app = builder.Build();
            /*var server = WebApp.Start(baseAddress, (appBuilder) =>
            {
                var config = new HttpConfiguration();
                config.Routes.MapHttpRoute(
                    "DefaultApi",
                    "{controller}/{action}");
                var builder = new ContainerBuilder();

                // Register Web API controller in executing assembly.
                builder.RegisterApiControllers(Assembly.GetExecutingAssembly());
                
                //builder.Register(c => modelManager).As<IMtProvider>().SingleInstance();
                builder.RegisterInstance<IMtProvider>(modelManager);
                // Create and assign a dependency resolver for Web API to use.
                var container = builder.Build();
                config.DependencyResolver = new AutofacWebApiDependencyResolver(container);

                appBuilder.UseAutofacWebApi(config);
                appBuilder.UseWebApi(config);
            });*/
        }
    }
}
