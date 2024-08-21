
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
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using LiveChartsCore;

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
                
                //First try to open the external http listener, this requires admin (or a prior
                //reservation of the port with netsh)
                try
                {
                    this.StartWebApp(baseAddress, modelManager);
                    Log.Information($"Started HTTP API at http://+:{OpusCatMtEngineSettings.Default.HttpMtServicePort}. This API can be accessed from remote computers, if the firewall has been configured to allow it.");
                }
                //If opening the external listener fails, open a localhost listener (works without admin).
                catch (Exception ex)
                {
                    this.StartWebApp($"http://localhost:{OpusCatMtEngineSettings.Default.HttpMtServicePort}", modelManager);
                    Log.Information($"Started HTTP API at http://localhost:{OpusCatMtEngineSettings.Default.HttpMtServicePort}. This API cannot be accessed from remote computers.");
                }
            }
            else
            {                
                baseAddress = $"http://localhost:{OpusCatMtEngineSettings.Default.HttpMtServicePort}";

                this.StartWebApp($"http://localhost:{OpusCatMtEngineSettings.Default.HttpMtServicePort}", modelManager);
                Log.Information($"Started HTTP API at http://localhost:{OpusCatMtEngineSettings.Default.HttpMtServicePort}. This API cannot be accessed from remote computers.");
            }

            

        }

        private void StartWebApp(string baseAddress, ModelManager modelManager)
        {
            var builder = WebApplication.CreateBuilder();
            builder.Services.AddControllers().AddJsonOptions(
                options => { options.JsonSerializerOptions.PropertyNamingPolicy = null; options.JsonSerializerOptions.PropertyNameCaseInsensitive = false; });
            builder.Services.AddSingleton<IMtProvider>(modelManager);
            
            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(
                    builder =>
                    {
                        builder.AllowAnyOrigin();
                        builder.AllowAnyHeader();
                        builder.AllowAnyMethod();
                    });
            });
;
            var app = builder.Build();
            app.UseCors();
            app.UseRouting();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller}/{action}");
            
            app.RunAsync(baseAddress);
            
        }
    }
}
