using Microsoft.ServiceFabric.Services.Remoting;
using Models.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;

namespace Contracts.Database
{
    [ServiceContract]
    public interface IAuthDBService : IService
    {
        [OperationContract]
        Task<LoginData> Login(LoginData loginData);
    }
}
