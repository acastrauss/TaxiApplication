using AzureStorageWrapper;
using AzureStorageWrapper.DTO;
using AzureStorageWrapper.Entities;
using Microsoft.ServiceFabric.Data;
using Models.Chat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaxiData.DataImplementations;

namespace TaxiData.DataServices
{
    internal class ChatMessagesDataService : BaseDataService<Models.Chat.ChatMessage, AzureStorageWrapper.Entities.ChatMessage>
    {
        public ChatMessagesDataService(
            AzureStorageWrapper<AzureStorageWrapper.Entities.ChatMessage> storageWrapper, 
            IDTOConverter<AzureStorageWrapper.Entities.ChatMessage, Models.Chat.ChatMessage> converter,
            Synchronizer<AzureStorageWrapper.Entities.ChatMessage, Models.Chat.ChatMessage> synchronizer, 
            IReliableStateManager stateManager) : 
            base(storageWrapper, converter, synchronizer, stateManager)
        {}

        public async Task<Models.Chat.ChatMessage> AddNewMessageToChat(Models.Chat.ChatMessage message)
        {
            var dict = await GetReliableDictionary();
            using var txWrapper = new StateManagerTransactionWrapper(stateManager.CreateTransaction());
            var msgKey = $"{message.UserEmail}{message.Timestamp}";
            var created = await dict.AddOrUpdateAsync(txWrapper.transaction, msgKey, message, (key, value) => value);
            return created;
        }

        public async Task<IEnumerable<Models.Chat.ChatMessage>> GetMessagesForChat(string clientEmail, string driverEmail, long rideCreatedAtTimestamp)
        {
            var dict = await GetReliableDictionary();
            using var txWrapper = new StateManagerTransactionWrapper(stateManager.CreateTransaction());

            var collectionEnum = await dict.CreateEnumerableAsync(txWrapper.transaction);
            var asyncEnum = collectionEnum.GetAsyncEnumerator();

            var msgs = new List<Models.Chat.ChatMessage>();

            while (await asyncEnum.MoveNextAsync(default))
            {
                var msg = asyncEnum.Current.Value;
                if(msg != null)
                {
                    if (msg.ClientEmail.Equals(clientEmail) && msg.DriverEmail.Equals(driverEmail) && msg.RideCreadtedAtTimestamp == rideCreatedAtTimestamp)
                    {
                        msgs.Add(msg);
                    }
                }
            }

            return msgs;
        }
    }
}
