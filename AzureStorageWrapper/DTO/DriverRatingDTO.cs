using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureStorageWrapper.DTO
{
    public class DriverRatingDTO : IDTOConverter<Entities.DriverRating, Models.UserTypes.DriverRating>
    {
        public Entities.DriverRating AppModelToAzure(Models.UserTypes.DriverRating appModel)
        {
            return new Entities.DriverRating()
            {
                ClientEmail = appModel.ClientEmail,
                DriverEmail = appModel.DriverEmail,
                PartitionKey = appModel.DriverEmail,
                Rating = appModel.Rating,
                RideTimestamp = appModel.RideTimestamp,
                RowKey = appModel.RideTimestamp.ToString(),
            };
        }

        public Models.UserTypes.DriverRating AzureToAppModel(Entities.DriverRating azureModel)
        {
            return new Models.UserTypes.DriverRating()
            {
                ClientEmail = azureModel.ClientEmail,
                DriverEmail = azureModel.DriverEmail,
                RideTimestamp = azureModel.RideTimestamp,
                Rating = azureModel.Rating,
            };
        }
    }
}
