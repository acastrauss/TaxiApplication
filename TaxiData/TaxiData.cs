using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Contracts.Database;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Models.Auth;
using AzureStorageWrapper.DTO;
using AzureStorageWrapper;
using Azure.Data.Tables;
using System.Diagnostics;
using Models.Blob;
using TaxiData.DataImplementations;
using Models.UserTypes;

namespace TaxiData
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class TaxiData : StatefulService, IAuthDBService 
    {
        private AzureStorageWrapper<AzureStorageWrapper.Entities.User> userTableStorageWrapper;
        private AzureStorageWrapper<AzureStorageWrapper.Entities.Driver> driverTableStorageWrapper;
        
        private IDTOConverter<AzureStorageWrapper.Entities.User, UserProfile> UserDTOConverter;
        private IDTOConverter<AzureStorageWrapper.Entities.Driver, Models.UserTypes.Driver> DriverDTOConverter;

        private readonly Synchronizer<AzureStorageWrapper.Entities.User, Models.Auth.UserProfile> userSync;
        private readonly Synchronizer<AzureStorageWrapper.Entities.Driver, Models.UserTypes.Driver> driverSync;

        public TaxiData(
            StatefulServiceContext context,
            AzureStorageWrapper<AzureStorageWrapper.Entities.User> userStorageWrapper,
            AzureStorageWrapper<AzureStorageWrapper.Entities.Driver> driverStorageWrapper
        )
            : base(context)
        {
            userTableStorageWrapper = userStorageWrapper;
            driverTableStorageWrapper = driverStorageWrapper;
            UserDTOConverter = new UserDTO();
            DriverDTOConverter = new DriverDTO();
            userSync = new Synchronizer<AzureStorageWrapper.Entities.User, UserProfile>(userStorageWrapper, typeof(UserProfile).Name, UserDTOConverter, StateManager);
            driverSync = new Synchronizer<AzureStorageWrapper.Entities.Driver, Models.UserTypes.Driver>(driverStorageWrapper, typeof(Models.UserTypes.Driver).Name, DriverDTOConverter, StateManager);
        }

        public async Task<bool> Exists(string partitionKey, string rowKey)
        {
            var usersDict = await StateManager.GetOrAddAsync<IReliableDictionary<string, UserProfile>>(typeof(UserProfile).Name);
            using var tx = StateManager.CreateTransaction();
            var existing = await usersDict.TryGetValueAsync(tx, $"{partitionKey}{rowKey}");
            await tx.CommitAsync();
            return existing.HasValue;
        } 

        public async Task<bool> ExistsWithPwd(string partitionKey, string rowKey, string password)
        {
            var usersDict = await StateManager.GetOrAddAsync<IReliableDictionary<string, UserProfile>>(typeof(UserProfile).Name);
            using var tx = StateManager.CreateTransaction();
            var existing = await usersDict.TryGetValueAsync(tx, $"{partitionKey}{rowKey}");
            await tx.CommitAsync();
            if (existing.HasValue)
            {
                return existing.Value.Password.Equals(password) &&
                    existing.Value.Email.Equals(rowKey) &&
                    existing.Value.Type.ToString().Equals(partitionKey);
            }
            return false;
        }

        public async Task<bool> ExistsSocialMediaAuth(string partitionKey, string rowKey)
        {
            var usersDict = await StateManager.GetOrAddAsync<IReliableDictionary<string, UserProfile>>(typeof(UserProfile).Name);
            using var tx = StateManager.CreateTransaction();
            var existing = await usersDict.TryGetValueAsync(tx, $"{partitionKey}{rowKey}");
            await tx.CommitAsync();
            if (existing.HasValue)
            {
                return existing.Value.Email.Equals(rowKey) &&
                    existing.Value.Type.ToString().Equals(partitionKey);
            }
            return false;
        }

        public async Task<bool> Create<T>(T appModel) where T: UserProfile
        {
            var dict = await StateManager.GetOrAddAsync<IReliableDictionary<string, T>>(typeof(T).Name);
            using var tx = StateManager.CreateTransaction();
            var dictKey = $"{appModel.Type.ToString()}{appModel.Email}";
            var created = await dict.AddOrUpdateAsync(tx, dictKey, appModel, (key, value) => value);
            await tx.CommitAsync();
            return created != null;
        }

        public async Task<bool> CreateUser(UserProfile appModel)
        {
            var userCreated = await Create(appModel);
            
            return userCreated;
        }
        public async Task<bool> CreateDriver(Driver appModel)
        {
            var userCreated = await Create<UserProfile>(appModel);
            if (userCreated)
            {
                var newDriver = new Models.UserTypes.Driver(appModel, Models.UserTypes.DriverStatus.NOT_VERIFIED);
                userCreated = await Create(newDriver);
            }

            return userCreated;
        }

        private async Task SyncAzureTablesWithDict()
        {
            await userSync.SyncAzureTablesWithDict();
            await driverSync.SyncAzureTablesWithDict();
        }

        private async Task RunPeriodicalUpdate(CancellationToken cancellationToken)
        {
            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                await SyncAzureTablesWithDict();

                await Task.Delay(TimeSpan.FromSeconds(20), cancellationToken);
            }
        }

        private async Task SyncDictWithAzureTable()
        {
            await userSync.SyncDictWithAzureTable();
            await driverSync.SyncDictWithAzureTable();
        }

        /// <summary>
        /// Optional override to create listeners (e.g., HTTP, Service Remoting, WCF, etc.) for this service replica to handle client or user requests.
        /// </summary>
        /// <remarks>
        /// For more information on service communication, see https://aka.ms/servicefabricservicecommunication
        /// </remarks>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return this.CreateServiceRemotingReplicaListeners();
        }

        /// <summary>
        /// This is the main entry point for your service replica.
        /// This method executes when this replica of your service becomes primary and has write status.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service replica.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            // TODO: Replace the following sample code with your own logic 
            //       or remove this RunAsync override if it's not needed in your service.


            var myDictionary = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, long>>("myDictionary");

            await SyncDictWithAzureTable();

            var periodicTask = RunPeriodicalUpdate(cancellationToken);

            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    await periodicTask;
                }

                cancellationToken.ThrowIfCancellationRequested();

                using (var tx = this.StateManager.CreateTransaction())
                {
                    var result = await myDictionary.TryGetValueAsync(tx, "Counter");

                    ServiceEventSource.Current.ServiceMessage(this.Context, "Current Counter Value: {0}",
                        result.HasValue ? result.Value.ToString() : "Value does not exist.");

                    await myDictionary.AddOrUpdateAsync(tx, "Counter", 0, (key, value) => ++value);

                    // If an exception is thrown before calling CommitAsync, the transaction aborts, all changes are 
                    // discarded, and nothing is saved to the secondary replicas.
                    await tx.CommitAsync();
                }

                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);

            }
        }
    }
}
