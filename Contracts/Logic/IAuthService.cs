using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using Microsoft.ServiceFabric.Services.Remoting;
using Models.Auth;

namespace Contracts.Logic
{
    [ServiceContract]
    public interface IAuthService : IService
    {
        [OperationContract]
        Task<Tuple<bool, UserType>> Login(LoginData loginData);

        [OperationContract]
        Task<bool> Register(UserProfile userProfile);
    }
}
