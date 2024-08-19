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
using TaxiData.DataImplementations;
using Models.UserTypes;
using Models.Ride;
using TaxiData.DataServices;
using Models.Chat;

namespace TaxiData
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class TaxiData : StatefulService, IAuthDBService
    {
        private readonly DataServiceFactory dataServiceFactory;

        public TaxiData(
            StatefulServiceContext context,
            AzureStorageWrapper.AzureStorageWrapper<AzureStorageWrapper.Entities.User> userStorageWrapper,
            AzureStorageWrapper.AzureStorageWrapper<AzureStorageWrapper.Entities.Driver> driverStorageWrapper,
            AzureStorageWrapper.AzureStorageWrapper<AzureStorageWrapper.Entities.Ride> rideStorageWrapper,
            AzureStorageWrapper.AzureStorageWrapper<AzureStorageWrapper.Entities.DriverRating> driverRatingWrapper,
            AzureStorageWrapper.AzureStorageWrapper<AzureStorageWrapper.Entities.Chat> chatStorageWrapper,
            AzureStorageWrapper.AzureStorageWrapper<AzureStorageWrapper.Entities.ChatMessage> chatMsgStorageWrapper
        )
            : base(context)
        {
            dataServiceFactory = new DataServiceFactory(
                StateManager,
                userStorageWrapper,
                driverStorageWrapper,
                rideStorageWrapper,
                driverRatingWrapper,
                chatStorageWrapper,
                chatMsgStorageWrapper
            );
        }

        #region AuthMethods
        public async Task<UserProfile> UpdateUserProfile(UpdateUserProfileRequest request, string partitionKey, string rowKey)
        {
            var usersDict = await StateManager.GetOrAddAsync<IReliableDictionary<string, UserProfile>>(typeof(UserProfile).Name);
            using var tx = StateManager.CreateTransaction();
            var key = $"{partitionKey}{rowKey}";
            var existing = await usersDict.TryGetValueAsync(tx, key);

            if (!existing.HasValue) 
            {
                return null;
            }

            if(request.Password != null)
            {
                existing.Value.Password = request.Password;
            }

            if (request.Username != null)
            {
                existing.Value.Username = request.Username;
            }

            if (request.Address != null)
            {
                existing.Value.Address = request.Address;
            }

            if (request.ImagePath != null)
            {
                existing.Value.ImagePath = request.ImagePath;
            }

            if (request.Fullname != null) 
            {
                existing.Value.Fullname = request.Fullname;
            }

            var updated = await usersDict.TryUpdateAsync(tx, key, existing.Value, existing.Value);

            await tx.CommitAsync();

            return updated ? existing.Value : null;
        }

        public async Task<UserProfile> GetUserProfile(string partitionKey, string rowKey)
        {
            return await dataServiceFactory.AuthDataService.GetUserProfile(partitionKey, rowKey);
        }

        public async Task<bool> Exists(string partitionKey, string rowKey)
        {
            return await dataServiceFactory.AuthDataService.Exists(partitionKey, rowKey);
        } 

        public async Task<bool> ExistsWithPwd(string partitionKey, string rowKey, string password)
        {
            return await dataServiceFactory.AuthDataService.ExistsWithPwd(partitionKey, rowKey, password);
        }

        public async Task<bool> ExistsSocialMediaAuth(string partitionKey, string rowKey)
        {
            return await dataServiceFactory.AuthDataService.ExistsSocialMediaAuth(partitionKey, rowKey);
        }
        public async Task<bool> CreateUser(UserProfile appModel)
        {
            return await dataServiceFactory.AuthDataService.Create(appModel);
        }
        public async Task<bool> CreateDriver(Models.UserTypes.Driver appModel)
        {
            var userCreated = await dataServiceFactory.AuthDataService.Create<UserProfile>(appModel);
            if (userCreated)
            {
                var newDriver = new Models.UserTypes.Driver(appModel, Models.UserTypes.DriverStatus.NOT_VERIFIED);
                userCreated = await dataServiceFactory.AuthDataService.Create(newDriver);
            }

            return userCreated;
        }

        #endregion

        #region SyncMethods

        private async Task SyncAzureTablesWithDict()
        {
            await dataServiceFactory.SyncAzureTablesWithDict();
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
            await dataServiceFactory.SyncDictWithAzureTable();
        }

        #endregion

        #region ServiceFabricMethods
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

        #endregion

        #region DriverMethods
        public async Task<DriverStatus> GetDriverStatus(string driverEmail)
        {
            return await dataServiceFactory.DriverDataService.GetDriverStatus(driverEmail);
        }

        public async Task<bool> UpdateDriverStatus(string driverEmail, DriverStatus status)
        {
            return await dataServiceFactory.DriverDataService.UpdateDriverStatus(driverEmail, status);
        }

        public async Task<IEnumerable<Models.UserTypes.Driver>> ListAllDrivers()
        {
            return await dataServiceFactory.DriverDataService.ListAllDrivers();
        }

        #endregion

        #region RideMethods

        public async Task<Models.Ride.Ride> CreateRide(Models.Ride.Ride ride)
        {
            return await dataServiceFactory.RideDataService.CreateRide(ride);
        }

        public async Task<Models.Ride.Ride> UpdateRide(UpdateRideRequest updateRide, string driverEmail)
        {
            return await dataServiceFactory.RideDataService.UpdateRide(updateRide, driverEmail);
        }

        public async Task<IEnumerable<Models.Ride.Ride>> GetRides(QueryRideParams? queryParams)
        {
            return await dataServiceFactory.RideDataService.GetRides(queryParams);    
        }

        public async Task<Ride> GetRide(string clientEmail, long rideCreatedAtTimestamp)
        {
            return await dataServiceFactory.RideDataService.GetRide(clientEmail, rideCreatedAtTimestamp);
        }
        #endregion

        #region ChatMethods
        public async Task<Chat> CreateNewOrGetExistingChat(Chat chat)
        {
            return await dataServiceFactory.ChatDataService.CreateNewOrGetExistingChat(chat);
        }

        public async Task<ChatMessage> AddNewMessageToChat(ChatMessage message)
        {
            return await dataServiceFactory.ChatMessagesDataService.AddNewMessageToChat(message);
        }

        #endregion

        #region DriverRatingMethods
        public async Task<Models.UserTypes.DriverRating> RateDriver(Models.UserTypes.DriverRating driverRating)
        {
            return await dataServiceFactory.DriverRatingDataService.RateDriver(driverRating);
        }

        public async Task<float> GetAverageRatingForDriver(string driverEmail)
        {
            return await dataServiceFactory.DriverRatingDataService.GetAverageRatingForDriver(driverEmail);
        }

        #endregion
    }
}
