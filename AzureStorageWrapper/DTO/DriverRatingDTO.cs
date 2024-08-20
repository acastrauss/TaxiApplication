using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureStorageWrapper.DTO
{
    public class DriverRatingDTO : IDTOConverter<Entities.RideRating, Models.UserTypes.RideRating>
    {
        public Entities.RideRating AppModelToAzure(Models.UserTypes.RideRating appModel)
        {
            return new Entities.RideRating()
            {
                ClientEmail = appModel.ClientEmail,
                DriverEmail = appModel.DriverEmail,
                PartitionKey = appModel.DriverEmail,
                Value = appModel.Value,
                RideTimestamp = appModel.RideTimestamp,
                RowKey = appModel.RideTimestamp.ToString(),
            };
        }

        public Models.UserTypes.RideRating AzureToAppModel(Entities.RideRating azureModel)
        {
            return new Models.UserTypes.RideRating()
            {
                ClientEmail = azureModel.ClientEmail,
                DriverEmail = azureModel.DriverEmail,
                RideTimestamp = azureModel.RideTimestamp,
                Value = azureModel.Value,
            };
        }
    }
}
