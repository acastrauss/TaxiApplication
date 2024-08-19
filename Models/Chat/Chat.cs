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
        public string ClientEmail { get; set; }
        [DataMember]
        [Required]
        public string DriverEmail { get; set; }
        [DataMember]
        [Required]
        public long RideCreatedAtTimestamp { get; set; }
        [DataMember]
        public List<ChatMessage> Messages { get; set; }
        [DataMember]
        public ChatStatus Status { get; set; }
    }

    [DataContract]
    public class ChatMessage
    {
        [DataMember]
        [Required]
        public string UserEmail { get; set; }
        [DataMember]
        [Required]
        public string Content { get; set; }
        [DataMember]
        [Required]
        public DateTime Timestamp { get; set; }
        
        // Keys from chat
        // TO DO: Try to remove these from app model
        [DataMember]
        [Required]
        public string ClientEmail { get; set; }
        [DataMember]
        [Required]
        public string DriverEmail { get; set; }
        [DataMember]
        [Required]
        public long RideCreadtedAtTimestamp { get; set; }
    }
}
