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
using AzureStorageWrapper.Entities;
using Azure.Data.Tables;
using System.Diagnostics;

namespace TaxiData
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class TaxiData : StatefulService, IAuthDBService 
    {
        private AzureStorageWrapper<User> storageWrapper;
        private IDTOConverter<User, UserProfile> DTOConverter;

        private readonly string usersDictionaryName = "usersDictionary";

        public TaxiData(StatefulServiceContext context, AzureStorageWrapper<User> storageWrapper, IDTOConverter<User, UserProfile> converter)
            : base(context)
        {
            this.storageWrapper = storageWrapper;
            this.DTOConverter = converter;
        }

        public async Task<bool> Exists(string partitionKey, string rowKey)
        {
            var usersDict = await StateManager.GetOrAddAsync<IReliableDictionary<string, UserProfile>>(usersDictionaryName);
            using var tx = StateManager.CreateTransaction();
            var existing = await usersDict.TryGetValueAsync(tx, $"{partitionKey}{rowKey}");
            await tx.CommitAsync();
            return existing.HasValue;
        } 

        public async Task<bool> Create(UserProfile appModel)
        {
            var dict = await StateManager.GetOrAddAsync<IReliableDictionary<string, UserProfile>>(usersDictionaryName);
            using var tx = StateManager.CreateTransaction();
            var dictKey = $"{appModel.Type.ToString()}{appModel.Email}";
            var created = await dict.AddOrUpdateAsync(tx, dictKey, appModel, (key, value) => value);
            await tx.CommitAsync();
            return created != null;
        }

        public async Task<bool> CreateUser(UserProfile appModel)
        {
            return await Create(appModel);
        }

        private async Task SyncAzureTablesWithDict()
        {
            // Finish sync with azure table storage
            var usersDict = await StateManager.GetOrAddAsync<IReliableDictionary<string, UserProfile>>(usersDictionaryName);
            using var tx = StateManager.CreateTransaction();
            var usersEnum = await usersDict.CreateEnumerableAsync(tx);
            var asyncEnum = usersEnum.GetAsyncEnumerator();
            var usersToSync = new List<User>();
            
            while (await asyncEnum.MoveNextAsync(default))
            {
                var user = asyncEnum.Current.Value;
                if (user != null)
                {
                    // Add to azure table
                    usersToSync.Add(DTOConverter.AppModelToAzure(user));
                }
            }

            await tx.CommitAsync();
            try
            {
                await storageWrapper.AddOrUpdateMultiple(usersToSync);
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.Message);
            }
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

                await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
            }
        }

        private async Task FillDictionaryWithExternalStorage()
        {
            var externalStorageEntities = storageWrapper.GetAll();
            if(externalStorageEntities == null)
            {
                return;
            }
            var dict = await StateManager.GetOrAddAsync<IReliableDictionary<string, UserProfile>>(usersDictionaryName);
            using var tx = StateManager.CreateTransaction();

            foreach (var exEntity in externalStorageEntities)
            {
                var appModel = DTOConverter.AzureToAppModel(exEntity);
                var dictKey = $"{appModel.Type}{appModel.Email}";
                var created = await dict.AddOrUpdateAsync(tx, dictKey, appModel, (key, value) => value);
            }

            await tx.CommitAsync();
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

            await FillDictionaryWithExternalStorage();

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
