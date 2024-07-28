using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureStorageWrapper.DTO
{
    public class DriverDTO : IDTOConverter<Entities.Driver, Models.UserTypes.Driver>
    {
        public Entities.Driver AppModelToAzure(Models.UserTypes.Driver appModel)
        {
            return new Entities.Driver(
                appModel.Username, appModel.Email, appModel.Password,
                appModel.Fullname, appModel.DateOfBirth, appModel.Address,
                appModel.Type, appModel.ImagePath, (int)appModel.Status
            );
        }

        public Models.UserTypes.Driver AzureToAppModel(Entities.Driver azureModel)
        {
            return new Models.UserTypes.Driver()
            {
                DateOfBirth = azureModel.DateOfBirth,
                Address = azureModel.Address,
                Email = azureModel.Email,
                Fullname = azureModel.Fullname,
                ImagePath = azureModel.ImagePath,
                Status = (Models.UserTypes.DriverStatus)azureModel.Status,
                Password = azureModel.Password,
                Type = (Models.Auth.UserType)azureModel.Type,
                Username = azureModel.Username
            };
        }
    }
}
