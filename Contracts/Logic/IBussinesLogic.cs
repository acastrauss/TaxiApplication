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
using Models.Email;

namespace Contracts.Logic
{
    [ServiceContract]
    public interface IBussinesLogic : IService
    {
        #region AuthAndUserMethods
        [OperationContract]
        Task<Tuple<bool, UserType>> Login(LoginData loginData);

        [OperationContract]
        Task<bool> Register(UserProfile userProfile);

        [OperationContract]
        Task<UserProfile> GetUserProfile(string userEmail, UserType userType);

        [OperationContract]
        Task<UserProfile> UpdateUserProfile(UpdateUserProfileRequest updateUserProfileRequest, string userEmail, UserType userType);

        #endregion

        #region DriverMethods
        [OperationContract]
        Task<DriverStatus> GetDriverStatus(string driverEmail);

        [OperationContract]
        Task<bool> UpdateDriverStatus(string driverEmail, DriverStatus status);

        [OperationContract]
        Task<IEnumerable<Driver>> ListAllDrivers();

        #endregion
        #region RideMethods

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

        [OperationContract]
        Task<Ride> GetRideStatus(string clientEmail, long rideCreatedAtTimestamp);

        #endregion
        #region EmailMethods

        [OperationContract]
        Task<bool> SendEmail(SendEmailRequest sendEmailRequest);
        #endregion
    }
}
