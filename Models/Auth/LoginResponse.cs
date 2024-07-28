using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Models.Auth
{
    [DataContract]
    public class LoginResponse
    {
        [DataMember]
        public bool Exists { get; set; }

        [DataMember]
        public string Email { get; set; }  
        [DataMember]
        public UserType Type { get; set; }

        [DataMember]
        public bool IsVerified { get; set; }
    }
}
