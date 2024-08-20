using AzureStorageWrapper;
using AzureStorageWrapper.DTO;
using Microsoft.ServiceFabric.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaxiData.DataImplementations;

namespace TaxiData.DataServices
{
    internal class DriverRatingDataService : BaseDataService<Models.UserTypes.RideRating, AzureStorageWrapper.Entities.RideRating>
    {
        public DriverRatingDataService(
            TablesOperations<AzureStorageWrapper.Entities.RideRating> storageWrapper, 
            IDTOConverter<AzureStorageWrapper.Entities.RideRating, 
            Models.UserTypes.RideRating> converter, 
            Synchronizer<AzureStorageWrapper.Entities.RideRating, 
            Models.UserTypes.RideRating> synchronizer, 
            IReliableStateManager stateManager
        ) : base(storageWrapper, converter, synchronizer, stateManager)
        {}

        public async Task<Models.UserTypes.RideRating> RateDriver(Models.UserTypes.RideRating driverRating)
        {
            var dict = await GetReliableDictionary();
            using var txWrapper = new StateManagerTransactionWrapper(stateManager.CreateTransaction());

            var key = $"{driverRating.DriverEmail}{driverRating.RideTimestamp}";
            var newRating = await dict.AddOrUpdateAsync(txWrapper.transaction, key, driverRating, (key, value) => value);

            return newRating;
        }

        public async Task<float> GetAverageRatingForDriver(string driverEmail)
        {
            var dict = await GetReliableDictionary();
            using var txWrapper = new StateManagerTransactionWrapper(stateManager.CreateTransaction());
            
            var collectionEnum = await dict.CreateEnumerableAsync(txWrapper.transaction);
            var asyncEnum = collectionEnum.GetAsyncEnumerator();

            float sum = 0.0f;
            int cnt = 0;

            while (await asyncEnum.MoveNextAsync(default))
            {
                var ratingEntity = asyncEnum.Current.Value;
                if (ratingEntity != null)
                {
                    if (ratingEntity.DriverEmail.Equals(driverEmail))
                    {
                        cnt += 1;
                        sum += ratingEntity.Value;
                    }
                }
            }

            if (cnt == 0)
            {
                return 0;
            }

            return sum / cnt;
        }
    }
}
