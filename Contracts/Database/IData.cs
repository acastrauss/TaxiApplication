﻿using Microsoft.ServiceFabric.Services.Remoting;
using Models.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using Models.UserTypes;
using Models.Ride;
using Models.Chat;

namespace Contracts.Database
{
    [ServiceContract]
    public interface IData : IService
    {
        [OperationContract]
        Task<bool> Exists(string partitionKey, string rowKey);

        [OperationContract]
        Task<bool> ExistsWithPwd(string partitionKey, string rowKey, string password);

        [OperationContract]
        Task<Models.Auth.UserProfile> GetUserProfile(string partitionKey, string rowKey);

        [OperationContract]
        Task<Models.Auth.UserProfile> UpdateUserProfile(UpdateUserProfileRequest request, string partitionKey, string rowKey);

        [OperationContract]
        Task<bool> CreateUser(UserProfile appModel);

        [OperationContract]
        Task<bool> CreateDriver(Driver appModel);

        [OperationContract]
        Task<bool> ExistsSocialMediaAuth(string partitionKey, string rowKey);

        [OperationContract]
        Task<DriverStatus> GetDriverStatus(string driverEmail);

        [OperationContract]
        Task<bool> UpdateDriverStatus(string driverEmail, DriverStatus status);

        [OperationContract]
        Task<IEnumerable<Driver>> ListAllDrivers();

        [OperationContract]
        Task<Models.Ride.Ride> CreateRide(Models.Ride.Ride ride);

        [OperationContract]
        Task<Models.Ride.Ride> UpdateRide(Models.Ride.UpdateRideRequest updateRide, string driverEmail);

        [OperationContract]
        Task<IEnumerable<Models.Ride.Ride>> GetRides(Models.Ride.QueryRideParams? queryParams);

        [OperationContract]
        Task<Ride> GetRide(string clientEmail, long rideCreatedAtTimestamp);

        [OperationContract]
        Task<Chat> CreateNewOrGetExistingChat(Models.Chat.Chat chat);

        [OperationContract] 
        Task<ChatMessage> AddNewMessageToChat(Models.Chat.ChatMessage message);

        [OperationContract]
        Task<RideRating> RateDriver(RideRating driverRating);

        [OperationContract]
        Task<float> GetAverageRatingForDriver(string driverEmail);
    }
}
