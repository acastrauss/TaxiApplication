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
using Models.Ride;
using Models.UserTypes;

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

        #region DriverMethods

        public async Task<DriverStatus> GetDriverStatus(string driverEmail)
        {
            return await authDBService.GetDriverStatus(driverEmail);
        }

        public async Task<bool> UpdateDriverStatus(string driverEmail, DriverStatus status)
        {
            return await authDBService.UpdateDriverStatus(driverEmail, status);
        }

        public async Task<IEnumerable<Driver>> ListAllDrivers()
        {
            return await authDBService.ListAllDrivers();
        }

        #endregion

        #region AuthMethods

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

        #endregion

        #region ServiceFabricMethods
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

        #endregion

        #region RideMethods

        public Task<EstimateRideResponse> EstimateRide(EstimateRideRequest request)
        {
            var randomGen = new Random();

            return Task.FromResult(
                new EstimateRideResponse()
                {
                    PriceEstimate = randomGen.NextSingle() * 1000,
                    TimeEstimateSeconds = randomGen.Next(60 * 60) // Max 1 hour
                });
        }

        public async Task<Ride> CreateRide(CreateRideRequest request, string clientEmail)
        {
            var newRide = new Models.Ride.Ride()
            {
                ClientEmail = clientEmail,
                CreatedAtTimestamp = DateTime.Now.Ticks,
                DriverEmail = null,
                EndAddress = request.EndAddress,
                StartAddress = request.StartAddress,
                Price = request.Price,
                Status = RideStatus.CREATED
            };

            return await authDBService.CreateRide(newRide);
        }

        public async Task<Ride> UpdateRide(UpdateRideRequest request, string driverEmail)
        {
            return await authDBService.UpdateRide(request, driverEmail);
        }

        public async Task<IEnumerable<Ride>> GetNewRides()
        {
            return await authDBService.GetRides(new QueryRideParams()
            {
                Status = RideStatus.CREATED
            });
        }
        #endregion
    }
}
