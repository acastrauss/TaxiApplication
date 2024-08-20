using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AzureStorageWrapper.Entities
{
    public class RideRating : AzureBaseEntity
    {
        public RideRating():base(string.Empty, string.Empty) {}
        public RideRating(string partitionKey, string rowKey) : base(partitionKey, rowKey) {}

        public string ClientEmail { get; set; }
        public long RideTimestamp { get; set; }
        public string DriverEmail { get; set; }
        public int Value { get; set; }
    }
}
