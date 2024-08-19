using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Models.Chat
{
    public enum ChatStatus
    {
        ACTIVE = 0,
        CLOSED = 1
    }

    [DataContract]
    public class Chat
    {
        [DataMember]
        [Required]
        public string clientEmail { get; set; }
        [DataMember]
        [Required]
        public string driverEmail { get; set; }
        [DataMember]
        [Required]
        public long rideCreatedAtTimestamp { get; set; }
        [DataMember]
        public List<ChatMessage> messages { get; set; }
        [DataMember]
        public ChatStatus status { get; set; }
    }

    [DataContract]
    public class ChatMessage
    {
        [DataMember]
        [Required]
        public string userEmail { get; set; }
        [DataMember]
        [Required]
        public string content { get; set; }
        [DataMember]
        [Required]
        public DateTime timestamp { get; set; }
        
        // Keys from chat
        // TO DO: Try to remove these from app model
        [DataMember]
        [Required]
        public string clientEmail { get; set; }
        [DataMember]
        [Required]
        public string driverEmail { get; set; }
        [DataMember]
        [Required]
        public long rideCreadtedAtTimestamp { get; set; }
    }
}
