using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Contracts.Database;
using Contracts.Logic;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Models.Auth;
using Models.Blob;

namespace TaxiMainLogic
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class TaxiMainLogic : StatelessService, IAuthService
    {
        private IAuthDBService authDBService;

        public TaxiMainLogic(StatelessServiceContext context, IAuthDBService authDBService)
            : base(context)
        {
            this.authDBService = authDBService;
        }

        public async Task<Tuple<bool, UserType>> Login(LoginData loginData)
        {
            bool exists = false;
            foreach (UserType type in Enum.GetValues(typeof(UserType)))
            {
                if (loginData.authType == AuthType.TRADITIONAL)
                {
                    exists |= await authDBService.ExistsWithPwd(type.ToString(), loginData.Email, loginData.Password);
                }
                else
                {
                    exists |= await authDBService.ExistsSocialMediaAuth(type.ToString(), loginData.Email);
                }

                if (exists)
                {
                    return Tuple.Create(exists, type);
                }
            }

            return Tuple.Create<bool, UserType>(exists, default);
        }

        public async Task<bool> Register(UserProfile userProfile)
        {
            var userExists = await authDBService.Exists(userProfile.Type.ToString(), userProfile.Email);

            if (userExists)
            {
                return false;
            }

            if(userProfile.Type == UserType.DRIVER)
            {
                var newDriver = new Models.UserTypes.Driver(userProfile, Models.UserTypes.DriverStatus.NOT_VERIFIED);
                return await authDBService.CreateDriver(newDriver);
            }

            return await authDBService.CreateUser(userProfile);
        }

        /// <summary>
        /// Optional override to create listeners (e.g., TCP, HTTP) for this service replica to handle client or user requests.
        /// </summary>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return this.CreateServiceRemotingInstanceListeners();
        }

        /// <summary>
        /// This is the main entry point for your service instance.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service instance.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            long iterations = 0;

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                ServiceEventSource.Current.ServiceMessage(this.Context, "Working-{0}", ++iterations);

                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
        }
    }
}
