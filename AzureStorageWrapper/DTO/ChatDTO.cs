using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureStorageWrapper.DTO
{
    public class ChatDTO : IDTOConverter<Entities.Chat, Models.Chat.Chat>
    {
        public Entities.Chat AppModelToAzure(Models.Chat.Chat appModel)
        {
            return new Entities.Chat()
            {
                ClientEmail = appModel.ClientEmail,
                DriverEmail = appModel.DriverEmail,
                PartitionKey = appModel.ClientEmail,
                RideCreadtedAtTimestamp = appModel.RideCreatedAtTimestamp,
                RowKey = appModel.RideCreatedAtTimestamp.ToString(),
                Status = (int)appModel.Status
            };
        }

        public Models.Chat.Chat AzureToAppModel(Entities.Chat azureModel)
        {
            return new Models.Chat.Chat()
            {
                ClientEmail = azureModel.ClientEmail,
                DriverEmail = azureModel.DriverEmail,
                RideCreatedAtTimestamp = azureModel.RideCreadtedAtTimestamp,
                Messages = new List<Models.Chat.ChatMessage>() { },
                Status = (Models.Chat.ChatStatus)azureModel.Status
            };
        }
    }
}
