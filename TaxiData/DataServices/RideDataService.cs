
using AzureStorageWrapper;
using AzureStorageWrapper.DTO;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using Models.Ride;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaxiData.DataImplementations;

namespace TaxiData.DataServices
{
    internal class RideDataService : BaseDataService<Models.Ride.Ride, AzureStorageWrapper.Entities.Ride>
    {
        public RideDataService(
            AzureStorageWrapper<AzureStorageWrapper.Entities.Ride> storageWrapper, 
            IDTOConverter<AzureStorageWrapper.Entities.Ride, Models.Ride.Ride> converter, 
            Synchronizer<AzureStorageWrapper.Entities.Ride, Models.Ride.Ride> synchronizer, 
            IReliableStateManager stateManager) : 
            base(storageWrapper, converter, synchronizer, stateManager)
        {}

        public async Task<Models.Ride.Ride> CreateRide(Models.Ride.Ride ride)
        {
            var dict = await GetReliableDictionary();
            using var txWrapper = new StateManagerTransactionWrapper(stateManager.CreateTransaction());

            var dictKey = $"{ride.ClientEmail}{ride.CreatedAtTimestamp}";
            var createdRide = await dict.AddOrUpdateAsync(txWrapper.transaction, dictKey, ride, (key, value) => value);
            return createdRide;
        }

        public async Task<Models.Ride.Ride> UpdateRide(UpdateRideRequest updateRide, string driverEmail)
        {
            var dict = await GetReliableDictionary();
            using var txWrapper = new StateManagerTransactionWrapper(stateManager.CreateTransaction());

            var rideKey = $"{updateRide.ClientEmail}{updateRide.RideCreatedAtTimestamp}";

            var existing = await dict.TryGetValueAsync(txWrapper.transaction, rideKey);

            if (!existing.HasValue)
            {
                return null;
            }

            existing.Value.Status = updateRide.Status;

            if (updateRide.Status == RideStatus.ACCEPTED && updateRide is UpdateRideWithTimeEstimate updateRideWithTime)
            {
                existing.Value.DriverEmail = driverEmail;
                existing.Value.EstimatedRideEnd = existing.Value.EstimatedDriverArrival.AddSeconds(updateRideWithTime.RideEstimateSeconds);
            }

            var res = await dict.AddOrUpdateAsync(txWrapper.transaction, rideKey, existing.Value, (key, value) => value);

            return res;
        }
        public async Task<IEnumerable<Models.Ride.Ride>> GetRides(QueryRideParams? queryParams)
        {
            var dict = await GetReliableDictionary();
            using var txWrapper = new StateManagerTransactionWrapper(stateManager.CreateTransaction());

            var collectionEnum = await dict.CreateEnumerableAsync(txWrapper.transaction);
            var asyncEnum = collectionEnum.GetAsyncEnumerator();
            var rides = new List<Models.Ride.Ride>();

            while (await asyncEnum.MoveNextAsync(default))
            {
                var rideEntity = asyncEnum.Current.Value;
                if (rideEntity != null)
                {
                    if (queryParams != null)
                    {
                        bool shouldAdd = (queryParams.Status == null) || (queryParams.Status == rideEntity.Status);
                        shouldAdd &= (queryParams.ClientEmail == null) || (queryParams.ClientEmail == rideEntity.ClientEmail);
                        shouldAdd &= (queryParams.DriverEmail == null) || (queryParams.DriverEmail == rideEntity.DriverEmail);
                        if (shouldAdd)
                        {
                            rides.Add(rideEntity);
                        }
                    }
                    else
                    {
                        rides.Add(rideEntity);
                    }
                }
            }

            return rides;
        }

        public async Task<Ride> GetRide(string clientEmail, long rideCreatedAtTimestamp)
        {
            var dict = await GetReliableDictionary();
            using var txWrapper = new StateManagerTransactionWrapper(stateManager.CreateTransaction());

            var existingRide = await dict.TryGetValueAsync(txWrapper.transaction, $"{clientEmail}{rideCreatedAtTimestamp}");

            if (!existingRide.HasValue)
            {
                return null;
            }

            return existingRide.Value;
        }
    }
}
