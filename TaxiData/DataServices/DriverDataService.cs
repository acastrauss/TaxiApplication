using AzureStorageWrapper;
using AzureStorageWrapper.DTO;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using Models.Auth;
using Models.UserTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaxiData.DataImplementations;

namespace TaxiData.DataServices
{
    internal class DriverDataService : BaseDataService<Models.UserTypes.Driver, AzureStorageWrapper.Entities.Driver>
    {
        public DriverDataService(
            AzureStorageWrapper<AzureStorageWrapper.Entities.Driver> storageWrapper, 
            IDTOConverter<AzureStorageWrapper.Entities.Driver, Models.UserTypes.Driver> converter, 
            Synchronizer<AzureStorageWrapper.Entities.Driver, Models.UserTypes.Driver> synchronizer,
            IReliableStateManager stateManager
        ) : base(storageWrapper, converter, synchronizer, stateManager)
        {}

        public async Task<DriverStatus> GetDriverStatus(string driverEmail)
        {
            var dict = await GetReliableDictionary();
            using var txWrapper = new StateManagerTransactionWrapper(stateManager.CreateTransaction());

            var existingDriver = await dict.TryGetValueAsync(txWrapper.transaction, $"{UserType.DRIVER}{driverEmail}");

            if (!existingDriver.HasValue)
            {
                return default;
            }

            return existingDriver.Value.Status;
        }

        public async Task<bool> UpdateDriverStatus(string driverEmail, DriverStatus status)
        {
            var dict = await GetReliableDictionary();
            using var txWrapper = new StateManagerTransactionWrapper(stateManager.CreateTransaction());

            var existingDriver = await dict.TryGetValueAsync(txWrapper.transaction, $"{UserType.DRIVER}{driverEmail}");
            if (!existingDriver.HasValue)
            {
                return false;
            }
            existingDriver.Value.Status = status;
            var result = await dict.TryUpdateAsync(txWrapper.transaction, $"{UserType.DRIVER}{driverEmail}", existingDriver.Value, existingDriver.Value);
            return result;
        }

        public async Task<IEnumerable<Models.UserTypes.Driver>> ListAllDrivers()
        {
            var dict = await GetReliableDictionary();
            using var txWrapper = new StateManagerTransactionWrapper(stateManager.CreateTransaction());

            var collectionEnum = await dict.CreateEnumerableAsync(txWrapper.transaction);
            var asyncEnum = collectionEnum.GetAsyncEnumerator();

            var drivers = new List<Models.UserTypes.Driver>();

            while (await asyncEnum.MoveNextAsync(default))
            {
                var driverEntity = asyncEnum.Current.Value;
                if (driverEntity != null)
                {
                    drivers.Add(driverEntity);
                }
            }

            return drivers;
        }


    }
}
