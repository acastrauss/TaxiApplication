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
                ClientEmail = appModel.clientEmail,
                DriverEmail = appModel.driverEmail,
                PartitionKey = appModel.clientEmail,
                RideCreadtedAtTimestamp = (long)appModel.rideCreatedAtTimestamp,
                RowKey = appModel.rideCreatedAtTimestamp.ToString(),
                Status = (int)appModel.status
            };
        }

        public Models.Chat.Chat AzureToAppModel(Entities.Chat azureModel)
        {
            return new Models.Chat.Chat()
            {
                clientEmail = azureModel.ClientEmail,
                driverEmail = azureModel.DriverEmail,
                rideCreatedAtTimestamp = azureModel.RideCreadtedAtTimestamp,
                messages = new List<Models.Chat.ChatMessage>() { },
                status = (Models.Chat.ChatStatus)azureModel.Status
            };
        }
    }
}
