using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using Microsoft.ServiceFabric.Services.Remoting;
using Models.Auth;
using Models.UserTypes;
using Models.Ride;

namespace Contracts.Logic
{
    [ServiceContract]
    public interface IAuthService : IService
    {
        [OperationContract]
        Task<Tuple<bool, UserType>> Login(LoginData loginData);

        [OperationContract]
        Task<bool> Register(UserProfile userProfile);

        [OperationContract]
        Task<UserProfile> GetUserProfile(string userEmail, UserType userType);

        [OperationContract]
        Task<UserProfile> UpdateUserProfile(UpdateUserProfileRequest updateUserProfileRequest, string userEmail, UserType userType);

        [OperationContract]
        Task<DriverStatus> GetDriverStatus(string driverEmail);

        [OperationContract]
        Task<bool> UpdateDriverStatus(string driverEmail, DriverStatus status);

        [OperationContract]
        Task<IEnumerable<Driver>> ListAllDrivers();

        [OperationContract]
        Task<EstimateRideResponse> EstimateRide(EstimateRideRequest request);

        [OperationContract]
        Task<Ride> CreateRide(CreateRideRequest request, string clientEmail);

        [OperationContract]
        Task<Ride> UpdateRide(UpdateRideRequest request, string driverEmail);

        [OperationContract]
        Task<IEnumerable<Ride>> GetNewRides();

        [OperationContract]
        Task<IEnumerable<Ride>> GetUsersRides(string userEmail, UserType userType);

        [OperationContract]
        Task<IEnumerable<Ride>> GetAllRides();

    }
}
