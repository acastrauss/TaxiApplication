using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Models.UserTypes
{
    [DataContract]
    public class RideRating
    {
        [DataMember]
        public string ClientEmail { get; set; }
        [DataMember]
        public long RideTimestamp { get; set; }
        [DataMember]
        public string DriverEmail { get; set; }
        [DataMember]
        [System.ComponentModel.DataAnnotations.Range(1, 5)]
        public int Value { get; set; }
    }
}
