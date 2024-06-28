using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureStorageWrapper.DTO
{
    public static class UserDTO
    {
        public static Models.Auth.UserProfile DbToApp(Entities.User user)
        {
            return new Models.Auth.UserProfile()
            {
                Address = user.Address,
                DateOfBirth = user.DateOfBirth,
                Email = user.Email,
                Fullname = user.Fullname,
                ImagePath = user.ImagePath,
                Password = user.Password,
                Type = (Models.Auth.UserType)user.Type,
                Username = user.Username
            };
        }

        public static Entities.User AppToDb(Models.Auth.UserProfile user)
        {
            return new Entities.User(user.Username, user.Email, user.Password, user.Fullname, user.DateOfBirth, user.Address, user.Type, user.ImagePath);
        }
    }
}
