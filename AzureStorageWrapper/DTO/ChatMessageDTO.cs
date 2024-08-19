using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureStorageWrapper.DTO
{
    public class ChatMessageDTO : IDTOConverter<Entities.ChatMessage, Models.Chat.ChatMessage>
    {
        public Entities.ChatMessage AppModelToAzure(Models.Chat.ChatMessage appModel)
        {
            return new Entities.ChatMessage()
            {
                ClientEmail = appModel.ClientEmail,
                Content = appModel.Content,
                PartitionKey = appModel.UserEmail,
                RideCreadtedAtTimestamp = appModel.RideCreadtedAtTimestamp,
                RowKey = new DateTimeOffset(appModel.Timestamp).ToUnixTimeMilliseconds().ToString(),
                Timestamp = appModel.Timestamp,
                UserEmail = appModel.UserEmail,
                DriverEmail = appModel.DriverEmail,
            };
        }

        public Models.Chat.ChatMessage AzureToAppModel(Entities.ChatMessage azureModel)
        {
            return new Models.Chat.ChatMessage()
            {
                UserEmail = azureModel.UserEmail,
                Timestamp = azureModel.Timestamp,
                ClientEmail = azureModel.ClientEmail,
                Content = azureModel.Content,
                RideCreadtedAtTimestamp = azureModel.RideCreadtedAtTimestamp,
                DriverEmail = azureModel.DriverEmail
            };
        }
    }
}
