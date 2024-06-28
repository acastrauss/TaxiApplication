using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Models.Auth
{
    public enum UserType
    {
        ADMIN = 0,
        CLIENT = 1,
        DRIVER = 2
    }

    [DataContract]
    public class UserProfile
    {
        [DataMember]
        public string Username { get; set; } = string.Empty;
        [DataMember]
        public string Email { get; set; } = string.Empty;
        [DataMember]
        public string Password { get; set; } = string.Empty;
        [DataMember]
        public string Fullname { get; set; } = string.Empty;
        [DataMember]
        public DateTime DateOfBirth { get; set; }
        [DataMember]
        public string Address { get; set; } = string.Empty;
        [DataMember]
        public UserType Type { get; set; }
        [DataMember]
        public string ImagePath { get; set; } = string.Empty;
    }
}
