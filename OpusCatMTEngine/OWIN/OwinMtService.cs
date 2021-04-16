using Autofac;
using Autofac.Integration.WebApi;
using Microsoft.Owin.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Owin;

namespace OpusCatMTEngine
{
    public class OwinMtService
    {
        public OwinMtService(ModelManager modelManager)
        {
            var baseAddress = "http://localhost:8501";
            var server = WebApp.Start(baseAddress, (appBuilder) =>
            {
                var config = new HttpConfiguration();
                config.Routes.MapHttpRoute(
                    "DefaultApi",
                    "api/{controller}");
                var builder = new ContainerBuilder();

                // Register Web API controller in executing assembly.
                builder.RegisterApiControllers(Assembly.GetExecutingAssembly());

                // Register a logger service to be used by the controller and middleware.
                //builder.Register(c => modelManager).As<IMtProvider>().SingleInstance();
                builder.RegisterInstance<IMtProvider>(modelManager);
                // Create and assign a dependency resolver for Web API to use.
                var container = builder.Build();
                config.DependencyResolver = new AutofacWebApiDependencyResolver(container);

                appBuilder.UseAutofacWebApi(config);
                appBuilder.UseWebApi(config);
            });
        }
    }
}
