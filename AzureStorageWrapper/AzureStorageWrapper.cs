using Azure.Data.Tables;

namespace AzureStorageWrapper
{
    public class AzureStorageWrapper<T> where T : class, ITableEntity
    {
        private readonly TableClient _tableClient;
        private string connectionString;
        private string tableName;

        public AzureStorageWrapper(string connectionString, string tableName)
        {
            this._tableClient = new TableClient(connectionString, tableName);
            this.connectionString = connectionString;
            this.tableName = tableName;
            Init();
        }

        protected virtual void Init()
        {
            this._tableClient.CreateIfNotExists();
        }

        public async Task<T> Create(T entity)
        {
            var res = await this._tableClient.AddEntityAsync(entity);
            if (res.IsError)
            {
                return default;
            }

            return entity;
        }

        public async Task<bool> ExistsByKeys(string patritionKey, string rowKey)
        {
            var res = await _tableClient.GetEntityIfExistsAsync<T>(patritionKey, rowKey, null);
            return res.HasValue;
        }
    }
}