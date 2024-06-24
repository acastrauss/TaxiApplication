using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Remoting;

namespace Contracts.Logic
{
    public interface IAuthService : IService
    {
        Task Ping();
    }
}
