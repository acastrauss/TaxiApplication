﻿using Models.UserTypes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
    [KnownType(typeof(Driver))]
    public class UserProfile
    {
        [DataMember]
        [Required]
        [MinLength(3)]
        public string Username { get; set; } = string.Empty;
        
        [DataMember]
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        [DataMember]
        [Required]
        [MinLength(64)] // SHA256
        [MaxLength(64)] 
        public string Password { get; set; } = string.Empty;
        
        [DataMember]
        [Required]
        [MinLength (3)]
        public string Fullname { get; set; } = string.Empty;
        
        [DataMember]
        [Required]
        [DataType(DataType.DateTime)]
        public DateTime DateOfBirth { get; set; }

        [DataMember]
        [Required]
        [MinLength(3)]
        public string Address { get; set; } = string.Empty;

        [DataMember]
        [Required]
        public UserType Type { get; set; }
        
        [DataMember]
        [Required]
        [DataType(DataType.ImageUrl)]
        public string ImagePath { get; set; } = string.Empty;
    }
}
