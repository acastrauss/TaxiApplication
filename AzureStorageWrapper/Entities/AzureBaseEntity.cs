using Azure;
using Azure.Data.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureStorageWrapper.Entities
{
    public abstract class AzureBaseEntity : ITableEntity
    {
        protected AzureBaseEntity(string partitionKey, string rowKey)
        {
            PartitionKey = partitionKey;
            RowKey = rowKey;
        }

        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        public virtual string GetStatefulDictKey()
        {
            return $"{PartitionKey}{RowKey}";
        }
    }
}
