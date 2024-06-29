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
                string connectionString = "AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;DefaultEndpointsProtocol=http;BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;QueueEndpoint=http://127.0.0.1:10001/devstoreaccount1;TableEndpoint=http://127.0.0.1:10002/devstoreaccount1;";

                AzureStorageWrapper.AzureStorageWrapper<AzureStorageWrapper.Entities.User> userStorageWrapper = new AzureStorageWrapper.AzureStorageWrapper<AzureStorageWrapper.Entities.User>(connectionString, "user");
                AzureStorageWrapper.DTO.IDTOConverter<AzureStorageWrapper.Entities.User, Models.Auth.UserProfile> converter =
                    new UserDTO();
    
                ServiceRuntime.RegisterServiceAsync("TaxiDataType",
                    context => new TaxiData(context, userStorageWrapper, converter)).GetAwaiter().GetResult();

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
