using Models.UserTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Models.Ride
{
    public enum RideStatus
    {
        CREATED = 0,
        ACCEPTED = 1,
        COMPLETED = 2
    }

    [DataContract]
    public class Ride
    {
        [DataMember]
        public long CreatedAtTimestamp { get; set; }
        [DataMember]
        public string StartAddress { get; set; }
        [DataMember]
        public string EndAddress { get; set; }
        [DataMember]
        public string ClientEmail { get; set; }
        [DataMember]
        public string? DriverEmail { get; set; }
        [DataMember]
        public RideStatus Status { get; set; }
        [DataMember]
        public float Price { get; set; }
        [DataMember]
        public DateTime EstimatedDriverArrival {  get; set; }
        [DataMember]
        public DateTime? EstimatedRideEnd { get; set; }

    }
}
