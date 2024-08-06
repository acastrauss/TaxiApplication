using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureStorageWrapper.DTO
{
    public class RideDTO : IDTOConverter<Entities.Ride, Models.Ride.Ride>
    {
        public Entities.Ride AppModelToAzure(Models.Ride.Ride appModel)
        {
            return new Entities.Ride()
            {
                ClientEmail = appModel.ClientEmail,
                CreatedAtTimestamp = appModel.CreatedAtTimestamp,
                DriverEmail = appModel.DriverEmail,
                EndAddress = appModel.EndAddress,
                PartitionKey = appModel.ClientEmail,
                Price = appModel.Price,
                RowKey = appModel.CreatedAtTimestamp.ToString(),
                StartAddress = appModel.StartAddress,
                Status = (int)appModel.Status,
                EstimatedDriverArrival = appModel.EstimatedDriverArrival,
                EstimatedRideEnd = appModel.EstimatedRideEnd,
            };
        }

        public Models.Ride.Ride AzureToAppModel(Entities.Ride azureModel)
        {
            return new Models.Ride.Ride()
            {
                ClientEmail = azureModel.ClientEmail,
                CreatedAtTimestamp = azureModel.CreatedAtTimestamp,
                DriverEmail = azureModel.DriverEmail,
                EndAddress = azureModel.EndAddress,
                Price = azureModel.Price,
                StartAddress = azureModel.StartAddress,
                Status = (Models.Ride.RideStatus)azureModel.Status,
                EstimatedDriverArrival=azureModel.EstimatedDriverArrival,
                EstimatedRideEnd=azureModel.EstimatedRideEnd,
            };
        }
    }
}
