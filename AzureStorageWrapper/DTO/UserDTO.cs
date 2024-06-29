using AzureStorageWrapper.Entities;
using Models.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureStorageWrapper.DTO
{
    public class UserDTO : IDTOConverter<Entities.User, Models.Auth.UserProfile>
    {
        public UserProfile AzureToAppModel(User azureModel)
        {
            return new Models.Auth.UserProfile()
            {
                Address = azureModel.Address,
                DateOfBirth = azureModel.DateOfBirth,
                Email = azureModel.Email,
                Fullname = azureModel.Fullname,
                ImagePath = azureModel.ImagePath,
                Password = azureModel.Password,
                Type = (Models.Auth.UserType)azureModel.Type,
                Username = azureModel.Username
            };
        }

        public User AppModelToAzure(UserProfile appModel)
        {
            return new Entities.User(appModel.Username, appModel.Email, appModel.Password, appModel.Fullname, appModel.DateOfBirth, appModel.Address, appModel.Type, appModel.ImagePath);
        }
    }
}
