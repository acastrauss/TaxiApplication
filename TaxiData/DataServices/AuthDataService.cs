using AzureStorageWrapper;
using AzureStorageWrapper.DTO;
using AzureStorageWrapper.Entities;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using Models.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaxiData.DataImplementations;

namespace TaxiData.DataServices
{
    internal class AuthDataService : BaseDataService<Models.Auth.UserProfile, AzureStorageWrapper.Entities.User>
    {
        public AuthDataService(
            TablesOperations<User> storageWrapper,
            IDTOConverter<User, UserProfile> converter,
            Synchronizer<User, UserProfile> synchronizer,
            IReliableStateManager stateManager
        )
            : base(storageWrapper, converter, synchronizer, stateManager)
        {}

        public async Task<UserProfile> UpdateUserProfile(UpdateUserProfileRequest request, string partitionKey, string rowKey)
        {
            var dict = await GetReliableDictionary();
            using var txWrapper = new StateManagerTransactionWrapper(stateManager.CreateTransaction());
            var key = $"{partitionKey}{rowKey}";
            var existing = await dict.TryGetValueAsync(txWrapper.transaction, key);

            if (!existing.HasValue)
            {
                return null;
            }

            if (request.Password != null)
            {
                existing.Value.Password = request.Password;
            }

            if (request.Username != null)
            {
                existing.Value.Username = request.Username;
            }

            if (request.Address != null)
            {
                existing.Value.Address = request.Address;
            }

            if (request.ImagePath != null)
            {
                existing.Value.ImagePath = request.ImagePath;
            }

            if (request.Fullname != null)
            {
                existing.Value.Fullname = request.Fullname;
            }

            var updated = await dict.TryUpdateAsync(txWrapper.transaction, key, existing.Value, existing.Value);

            return updated ? existing.Value : null;
        }
    
        public async Task<UserProfile> GetUserProfile(string partitionKey, string rowKey)
        {
            var dict = await GetReliableDictionary();
            using var txWrapper = new StateManagerTransactionWrapper(stateManager.CreateTransaction());
            var existing = await dict.TryGetValueAsync(txWrapper.transaction, $"{partitionKey}{rowKey}");
            return existing.Value;
        }

        public async Task<bool> Exists(string partitionKey, string rowKey)
        {
            var userProfile = await GetUserProfile(partitionKey, rowKey);
            return userProfile != null;
        }

        public async Task<bool> ExistsWithPwd(string partitionKey, string rowKey, string password)
        {
            var dict = await GetReliableDictionary();
            using var txWrapper = new StateManagerTransactionWrapper(stateManager.CreateTransaction());
            var existing = await dict.TryGetValueAsync(txWrapper.transaction, $"{partitionKey}{rowKey}");
            if (existing.HasValue)
            {
                return existing.Value.Password.Equals(password) &&
                    existing.Value.Email.Equals(rowKey) &&
                    existing.Value.Type.ToString().Equals(partitionKey);
            }
            return false;
        }

        public async Task<bool> ExistsSocialMediaAuth(string partitionKey, string rowKey)
        {
            var dict = await GetReliableDictionary();
            using var txWrapper = new StateManagerTransactionWrapper(stateManager.CreateTransaction());
            var existing = await dict.TryGetValueAsync(txWrapper.transaction, $"{partitionKey}{rowKey}");
            if (existing.HasValue)
            {
                return existing.Value.Email.Equals(rowKey) &&
                    existing.Value.Type.ToString().Equals(partitionKey);
            }
            return false;
        }

        public async Task<bool> Create<T>(T appModel) where T : UserProfile
        {
            var dict = await GetReliableDictionary();
            using var txWrapper = new StateManagerTransactionWrapper(stateManager.CreateTransaction());
            var dictKey = $"{appModel.Type}{appModel.Email}";
            var created = await dict.AddOrUpdateAsync(txWrapper.transaction, dictKey, appModel, (key, value) => value);
            return created != null;
        }
    }
}
