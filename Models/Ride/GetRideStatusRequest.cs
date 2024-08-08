using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Ride
{
    public class GetRideStatusRequest
    {
        public string ClientEmail { get; set; }
        public long RideCreatedAtTimestamp { get; set; }
    }
}
