using Models.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureStorageWrapper.Entities
{
    public class Driver : User
    {
        public int Status { get; set; }

        public Driver(string username, string email, string password, string fullname, DateTime dateOfBirth, string address, UserType type, string imagePath, int status) : 
            base(username, email, password, fullname, dateOfBirth, address, type, imagePath)
        {
            Status = status;
        }

        public Driver() : base() 
        {
            Status = 0;
        }
    }
}
