using AzureStorageWrapper;
using AzureStorageWrapper.DTO;
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
    internal class ChatDataService : BaseDataService<Models.Chat.Chat, AzureStorageWrapper.Entities.Chat>
    {
        private readonly ChatMessagesDataService messagesDataService;
        public ChatDataService(
            AzureStorageWrapper<AzureStorageWrapper.Entities.Chat> storageWrapper, 
            IDTOConverter<AzureStorageWrapper.Entities.Chat, Models.Chat.Chat> converter, 
            Synchronizer<AzureStorageWrapper.Entities.Chat, Models.Chat.Chat> synchronizer, 
            IReliableStateManager stateManager,
            ChatMessagesDataService messagesDataService) :
            base(storageWrapper, converter, synchronizer, stateManager)
        {
            this.messagesDataService = messagesDataService;
        }

        public async Task<Chat> CreateNewOrGetExistingChat(Models.Chat.Chat chat)
        {
            var dict = await GetReliableDictionary();
            using var txWrapper = new StateManagerTransactionWrapper(stateManager.CreateTransaction());
            var chatKey = $"{chat.ClientEmail}{chat.RideCreatedAtTimestamp}";

            var existing = await dict.TryGetValueAsync(txWrapper.transaction, chatKey);

            if (existing.HasValue)
            {
                var msgs = await messagesDataService.GetMessagesForChat(chat.ClientEmail, chat.DriverEmail, chat.RideCreatedAtTimestamp);
                if(msgs != null)
                {
                    existing.Value.Messages = msgs.ToList();
                }
                return existing.Value;
            }

            var created = await dict.AddOrUpdateAsync(txWrapper.transaction, chatKey, chat, (key, value) => value);
            return created;
        }
    }
}
