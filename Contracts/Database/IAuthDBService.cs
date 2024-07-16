using Microsoft.ServiceFabric.Services.Remoting;
using Models.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using Models.Blob;

namespace Contracts.Database
{
    [ServiceContract]
    public interface IAuthDBService : IService
    {
        [OperationContract]
        Task<bool> Exists(string partitionKey, string rowKey);

        [OperationContract]
        Task<bool> ExistsWithPwd(string partitionKey, string rowKey, string password);

        [OperationContract]
        Task<bool> CreateUser(UserProfile appModel);

        [OperationContract]
        Task<bool> ExistsSocialMediaAuth(string partitionKey, string rowKey);
    }
}
