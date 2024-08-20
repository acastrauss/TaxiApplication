using AzureStorageWrapper.DTO;
using AzureStorageWrapper.Entities;
using Microsoft.ServiceFabric.Data;
using Models;
using Models.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaxiData.DataServices
{
    internal class DataServiceFactory
    {
        public AuthDataService AuthDataService { get; private set; }
        public DriverDataService DriverDataService { get; private set; }
        public RideDataService RideDataService { get; private set; }
        public ChatDataService ChatDataService { get; private set; }
        public ChatMessagesDataService ChatMessagesDataService { get; private set; }
        public DriverRatingDataService DriverRatingDataService { get; private set; }

        public DataServiceFactory(
            IReliableStateManager stateManager,
            AzureStorageWrapper.TablesOperations<AzureStorageWrapper.Entities.User> userStorageWrapper,
            AzureStorageWrapper.TablesOperations<AzureStorageWrapper.Entities.Driver> driverStorageWrapper,
            AzureStorageWrapper.TablesOperations<AzureStorageWrapper.Entities.Ride> rideStorageWrapper,
            AzureStorageWrapper.TablesOperations<AzureStorageWrapper.Entities.RideRating> driverRatingStorageWrapper,
            AzureStorageWrapper.TablesOperations<AzureStorageWrapper.Entities.Chat> chatStorageWrapper,
            AzureStorageWrapper.TablesOperations<AzureStorageWrapper.Entities.ChatMessage> chatMessageStorageWrapper
        ) 
        {
            var userDto = new UserDTO();
            AuthDataService = new AuthDataService(
                userStorageWrapper,
                userDto,
                new DataImplementations.Synchronizer<User, Models.Auth.UserProfile>(
                    userStorageWrapper, 
                    typeof(UserProfile).Name, 
                    userDto, 
                    stateManager
                ),
                stateManager
            );
            var driverDto = new DriverDTO();
            DriverDataService = new DriverDataService(
                driverStorageWrapper,
                driverDto,
                new DataImplementations.Synchronizer<Driver, Models.UserTypes.Driver>(
                    driverStorageWrapper,
                    typeof(Driver).Name,
                    driverDto,
                    stateManager
                ),
                stateManager
            );

            var rideDto = new RideDTO();
            RideDataService = new RideDataService(
                rideStorageWrapper,
                rideDto,
                new DataImplementations.Synchronizer<Ride, Models.Ride.Ride>(
                    rideStorageWrapper,
                    typeof(Ride).Name,
                    rideDto,
                    stateManager
                ),
                stateManager
            );

            var driverRatingDto = new DriverRatingDTO();
            DriverRatingDataService = new DriverRatingDataService(
                driverRatingStorageWrapper,
                driverRatingDto,
                new DataImplementations.Synchronizer<RideRating, Models.UserTypes.RideRating>(
                    driverRatingStorageWrapper,
                    typeof(RideRating).Name,
                    driverRatingDto,
                    stateManager
                ),
                stateManager
            );

            var chatMsgDto = new ChatMessageDTO();
            ChatMessagesDataService = new ChatMessagesDataService(
                chatMessageStorageWrapper,
                chatMsgDto,
                new DataImplementations.Synchronizer<ChatMessage, Models.Chat.ChatMessage>(
                    chatMessageStorageWrapper,
                    typeof(ChatMessage).Name,
                    chatMsgDto,
                    stateManager
                ),
                stateManager
            );

            var chatDto = new ChatDTO();
            ChatDataService = new ChatDataService(
                chatStorageWrapper,
                chatDto,
                new DataImplementations.Synchronizer<Chat, Models.Chat.Chat>(
                    chatStorageWrapper,
                    typeof(Chat).Name,
                    chatDto,
                    stateManager
                ),
                stateManager,
                ChatMessagesDataService
            );
        }

        public async Task SyncAzureTablesWithDict()
        {
            await AuthDataService.SyncAzureTablesWithDict();
            await DriverDataService.SyncAzureTablesWithDict();
            await RideDataService.SyncAzureTablesWithDict();
            await DriverDataService.SyncAzureTablesWithDict();
            await ChatDataService.SyncAzureTablesWithDict();
            await ChatMessagesDataService.SyncAzureTablesWithDict();
        }
        public async Task SyncDictWithAzureTable()
        {
            await AuthDataService.SyncDictWithAzureTable();
            await DriverDataService.SyncDictWithAzureTable();
            await RideDataService.SyncDictWithAzureTable();
            await DriverDataService.SyncDictWithAzureTable();
            await ChatDataService.SyncDictWithAzureTable();
            await ChatMessagesDataService.SyncDictWithAzureTable();
        }
    }
}
