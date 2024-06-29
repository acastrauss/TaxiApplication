using Azure;
using Azure.Data.Tables;
using Models.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureStorageWrapper.Entities
{
    public class User : AzureBaseEntity
    {
        public User(string username, string email, string password, string fullname, DateTime dateOfBirth, string address, UserType type, string imagePath) : base(Enum.GetName(typeof(UserType), type)!, email)
        {
            Username = username;
            Email = email;
            Password = password;
            Fullname = fullname;
            DateOfBirth = DateTime.SpecifyKind(dateOfBirth, DateTimeKind.Utc);
            Address = address;
            Type = (int)type;
            ImagePath = imagePath;
        }

        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Fullname { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; private set; }
        public string Address { get; set; } = string.Empty;
        public int Type { get; set; }
        public string ImagePath { get; set; } = string.Empty;
    }
}
