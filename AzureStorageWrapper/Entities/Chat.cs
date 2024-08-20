using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureStorageWrapper.Entities
{
    public class Chat : AzureBaseEntity
    {
        public Chat(): base(string.Empty, string.Empty) { }

        // Partitio key
        public string ClientEmail { get; set; }
        public string DriverEmail { get; set; }
        // Row key
        public long RideCreadtedAtTimestamp { get; set; }
        public int Status { get; set; }
    }

    public class ChatMessage : AzureBaseEntity
    {
        public ChatMessage() : base(string.Empty, string.Empty) {}
        // Partition key
        public string UserEmail { get; set; }
        public string Content { get; set; }
        // Row key
        public DateTime Timestamp { get; set; }

        // Keys from chat
        public string ClientEmail { get; set; }
        public string DriverEmail { get; set; }
        public long RideCreadtedAtTimestamp { get; set; }
    }
}
