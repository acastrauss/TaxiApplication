using System;
using System.Diagnostics;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using Contracts.Database;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Microsoft.ServiceFabric.Services.Runtime;
using Models.Auth;
using TaxiMainLogic.Email;

namespace TaxiMainLogic
{
    internal static class Program
    {
        /// <summary>
        /// This is the entry point of the service host process.
        /// </summary>
        private static void Main()
        {
            try
            {
                // The ServiceManifest.XML file defines one or more service type names.
                // Registering a service maps a service type name to a .NET type.
                // When Service Fabric creates an instance of this service type,
                // an instance of the class is created in this host process.
                
                var authDBProxy = ServiceProxy.Create<IAuthDBService>(new Uri("fabric:/TaxiApplication/TaxiData"),
                    new Microsoft.ServiceFabric.Services.Client.ServicePartitionKey(1));

                ServiceRuntime.RegisterServiceAsync("TaxiMainLogicType",
                    context => {
                        var gmailConfigSection = context.CodePackageActivationContext
                        .GetConfigurationPackageObject("Config")
                        .Settings.Sections["GmailConfig"];

                        var gmailAppPassword = gmailConfigSection.Parameters["GmailAppPassword"].Value;
                        var gmailSendFromMail = gmailConfigSection.Parameters["GmailSendFrom"].Value;
                        var emailService = new EmailService(gmailSendFromMail, gmailAppPassword);

                        return new TaxiMainLogic(context, authDBProxy, emailService);
                    }).GetAwaiter().GetResult();

                ServiceEventSource.Current.ServiceTypeRegistered(Process.GetCurrentProcess().Id, typeof(TaxiMainLogic).Name);

                // Prevents this host process from terminating so services keep running.
                Thread.Sleep(Timeout.Infinite);
            }
            catch (Exception e)
            {
                ServiceEventSource.Current.ServiceHostInitializationFailed(e.ToString());
                throw;
            }
        }
    }
}
