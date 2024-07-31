using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Models.Ride
{
    [DataContract]
    public class TimeEstimate
    {
        [DataMember]
        public int Hours { get; set; }
        
        [DataMember]
        public int Minutes { get; set; }
        
        [DataMember]
        public int Seconds { get; set; }
    }

    public class EstimateRideResponse
    {
        public TimeEstimate TimeEstimate { get; set; }
        public float PriceEstimate { get; set; }
    }
}
