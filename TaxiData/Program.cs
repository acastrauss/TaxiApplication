using System;
using System.Diagnostics;
using System.Fabric.Management.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using AzureStorageWrapper.DTO;
using Microsoft.ServiceFabric.Services.Runtime;

namespace TaxiData
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

                ServiceRuntime.RegisterServiceAsync("TaxiDataType",
                    context =>
                    {
                        var azureTableConnString = context.CodePackageActivationContext.
                            GetConfigurationPackageObject("Config")
                            .Settings.Sections["Database"]
                            .Parameters["AzureTableConnectionString"].Value;

                        var userStorageWrapper = 
                            new AzureStorageWrapper.TablesOperations<AzureStorageWrapper.Entities.User>(azureTableConnString, "user");

                        var driverStorageWrapper =
                            new AzureStorageWrapper.TablesOperations<AzureStorageWrapper.Entities.Driver>(azureTableConnString, "driver");

                        var rideStorageWrapper =
                            new AzureStorageWrapper.TablesOperations<AzureStorageWrapper.Entities.Ride>(azureTableConnString, "ride");

                        var driverRatingStorageWrapper =
                            new AzureStorageWrapper.TablesOperations<AzureStorageWrapper.Entities.RideRating>(azureTableConnString, "rating");

                        var chatStorageWrapper =
                            new AzureStorageWrapper.TablesOperations<AzureStorageWrapper.Entities.Chat>(azureTableConnString, "chat");

                        var chatMsgStorageWrapper =
                            new AzureStorageWrapper.TablesOperations<AzureStorageWrapper.Entities.ChatMessage>(azureTableConnString, "chatMessages");

                        return new TaxiData(context, userStorageWrapper, driverStorageWrapper, rideStorageWrapper, driverRatingStorageWrapper, chatStorageWrapper, chatMsgStorageWrapper);
                    }
                    
                    
                    ).GetAwaiter().GetResult();

                var serviceTypeName = typeof(TaxiData).Name;

                ServiceEventSource.Current.ServiceTypeRegistered(Process.GetCurrentProcess().Id, serviceTypeName);

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
