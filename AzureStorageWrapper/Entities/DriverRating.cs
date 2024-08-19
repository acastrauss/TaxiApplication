using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AzureStorageWrapper.Entities
{
    public class DriverRating : AzureBaseEntity
    {
        public DriverRating():base(string.Empty, string.Empty) {}
        public DriverRating(string partitionKey, string rowKey) : base(partitionKey, rowKey) {}

        public string ClientEmail { get; set; }
        public long RideTimestamp { get; set; }
        public string DriverEmail { get; set; }
        public int Rating { get; set; }
    }
}
