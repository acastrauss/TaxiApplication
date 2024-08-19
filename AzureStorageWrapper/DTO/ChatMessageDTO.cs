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
                ClientEmail = appModel.clientEmail,
                Content = appModel.content,
                PartitionKey = appModel.userEmail,
                RideCreadtedAtTimestamp = appModel.rideCreadtedAtTimestamp,
                RowKey = new DateTimeOffset(appModel.timestamp).ToUnixTimeMilliseconds().ToString(),
                Timestamp = appModel.timestamp,
                UserEmail = appModel.userEmail,
                DriverEmail = appModel.driverEmail,
            };
        }

        public Models.Chat.ChatMessage AzureToAppModel(Entities.ChatMessage azureModel)
        {
            return new Models.Chat.ChatMessage()
            {
                userEmail = azureModel.UserEmail,
                timestamp = azureModel.Timestamp,
                clientEmail = azureModel.ClientEmail,
                content = azureModel.Content,
                rideCreadtedAtTimestamp = azureModel.RideCreadtedAtTimestamp,
                driverEmail = azureModel.DriverEmail
            };
        }
    }
}
