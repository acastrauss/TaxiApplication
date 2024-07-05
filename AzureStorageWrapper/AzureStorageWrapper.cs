using Azure;
using Azure.Data.Tables;
using Azure.Storage;
using AzureStorageWrapper.Entities;
using System.Collections.Concurrent;
using System.Xml;

namespace AzureStorageWrapper
{
    public class AzureStorageWrapper<T> where T : class, ITableEntity
    {
        private readonly TableClient tableClient;
        private string connectionString;
        private string tableName;

        public AzureStorageWrapper(string connectionString, string tableName)
        {
            this.tableClient = new TableClient(connectionString, tableName);
            this.connectionString = connectionString;
            this.tableName = tableName;
            Init();
        }

        protected virtual void Init()
        {
            this.tableClient.CreateIfNotExists();
        }

        public async Task<T> Create(T entity)
        {
            var res = await this.tableClient.AddEntityAsync(entity);
            if (res.IsError)
            {
                return default;
            }

            return entity;
        }

        public async Task AddOrUpdateMultiple(IEnumerable<T> entities)
        {
            foreach (var entity in entities)
            {
                var existing = await ExistsByKeys(entity.PartitionKey, entity.RowKey);
                if (existing != null)
                {
                    await tableClient.UpdateEntityAsync(entity, existing.ETag, TableUpdateMode.Replace);
                }
                else
                {
                    await tableClient.AddEntityAsync(entity);
                }
            }
        }

        public async Task<T> ExistsByKeys(string patritionKey, string rowKey)
        {
            var res = await tableClient.GetEntityIfExistsAsync<T>(patritionKey, rowKey);
            if(res == null)
            {
                return default;
            }

            return res.HasValue ? res.Value : default;
        }

        public IEnumerable<T> GetAll()
        {
            return tableClient.Query<T>().AsEnumerable();
        }
    }
}