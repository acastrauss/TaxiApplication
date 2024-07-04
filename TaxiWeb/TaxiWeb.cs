using System;
using System.Collections.Generic;
using System.Fabric;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.ServiceFabric.Services.Communication.AspNetCore;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Microsoft.ServiceFabric.Data;
using Contracts.Logic;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using TaxiWeb.Models;

namespace TaxiWeb
{
    /// <summary>
    /// The FabricRuntime creates an instance of this class for each service type instance.
    /// </summary>
    internal sealed class TaxiWeb : StatelessService
    {
        public TaxiWeb(StatelessServiceContext context)
            : base(context)
        {
        }

        /// <summary>
        /// Optional override to create listeners (like tcp, http) for this service instance.
        /// </summary>
        /// <returns>The collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new ServiceInstanceListener[]
            {
                new ServiceInstanceListener(serviceContext =>
                    new KestrelCommunicationListener(serviceContext, "ServiceEndpoint", (url, listener) =>
                    {
                        ServiceEventSource.Current.ServiceMessage(serviceContext, $"Starting Kestrel on {url}");

                        var builder = WebApplication.CreateBuilder();

                        builder.WebHost
                                    .UseKestrel()
                                    .ConfigureAppConfiguration((hostingContext, config) =>
                                    {
                                        var env = hostingContext.HostingEnvironment;

                                        config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                                            .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);

                                        if (env.IsDevelopment())
                                        {
                                            var appAssembly = Assembly.Load(new AssemblyName(env.ApplicationName));
                                            if (appAssembly != null)
                                            {
                                                config.AddUserSecrets(appAssembly, optional: true);
                                            }
                                        }

                                        config.AddEnvironmentVariables();
                                    })
                                    .UseContentRoot(Directory.GetCurrentDirectory())
                                    .UseServiceFabricIntegration(listener, ServiceFabricIntegrationOptions.None)
                                    .UseUrls(url);

                        builder.Services.Configure<JWTConfig>(builder.Configuration.GetSection("JWT"));
                        var jwtSecret = builder.Configuration.GetSection("JWT").GetValue<string>("Secret");

                        builder.Services.AddSingleton<StatelessServiceContext>(serviceContext);
                        var proxy = ServiceProxy.Create<IAuthService>(new Uri("fabric:/TaxiApplication/TaxiMainLogic"));
                        builder.Services.AddSingleton<IAuthService>(proxy);
                        // TO DO: Add to config
                        var key = Encoding.ASCII.GetBytes(jwtSecret);
                        builder.Services.AddAuthentication(x =>
                        {
                            x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                            x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                        })
                        .AddJwtBearer(x =>
                        {
                            x.RequireHttpsMetadata = false;
                            x.SaveToken = false;
                            x.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                            {
                                ValidateIssuerSigningKey = true,
                                IssuerSigningKey = new SymmetricSecurityKey(key),
                                ValidateIssuer = false,
                                ValidateAudience = false
                            };
                        });
                        
                        
                        // Add services to the container.
                        
                        builder.Services.AddControllers();
                        
                        var app = builder.Build();
                        // Configure the HTTP request pipeline.
                        
                        app.UseAuthorization();
                        
                        app.MapControllers();
                        
                        
                        return app;


                    }))
            };
        }

        
    }
}
