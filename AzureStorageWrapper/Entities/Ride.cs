using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureStorageWrapper.Entities
{
    public class Ride : AzureBaseEntity
    {
        public Ride(): base(string.Empty, string.Empty) { }
        public Ride(string partitionKey, string rowKey): base(partitionKey, rowKey) { }
    
        // Row Key
        public long CreatedAtTimestamp { get; set; }
        public string StartAddress {  get; set; }
        public string EndAddress {  get; set; }
        // Partition Key
        public string ClientEmail { get; set; }
        public string? DriverEmail { get; set; }
        public int Status { get; set; }
        public float Price { get; set; }
        public DateTime EstimatedDriverArrival { get; set; }
        public DateTime? EstimatedRideEnd { get; set; }
    }
}
