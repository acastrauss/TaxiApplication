﻿using AzureStorageWrapper.DTO;
using AzureStorageWrapper.Entities;
using Microsoft.ServiceFabric.Data;
using Models;
using Models.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaxiData.DataServices
{
    internal class DataServiceFactory
    {
        public AuthDataService AuthDataService { get; private set; }
        public DriverDataService DriverDataService { get; private set; }
        public RideDataService RideDataService { get; private set; }

        public DataServiceFactory(
            IReliableStateManager stateManager,
            AzureStorageWrapper.AzureStorageWrapper<AzureStorageWrapper.Entities.User> userStorageWrapper,
            AzureStorageWrapper.AzureStorageWrapper<AzureStorageWrapper.Entities.Driver> driverStorageWrapper,
            AzureStorageWrapper.AzureStorageWrapper<AzureStorageWrapper.Entities.Ride> rideStorageWrapper
        ) 
        {
            var userDto = new UserDTO();
            AuthDataService = new AuthDataService(
                userStorageWrapper,
                userDto,
                new DataImplementations.Synchronizer<User, Models.Auth.UserProfile>(
                    userStorageWrapper, 
                    typeof(UserProfile).Name, 
                    userDto, 
                    stateManager
                ),
                stateManager
            );
            var driverDto = new DriverDTO();
            DriverDataService = new DriverDataService(
                driverStorageWrapper,
                driverDto,
                new DataImplementations.Synchronizer<Driver, Models.UserTypes.Driver>(
                    driverStorageWrapper,
                    typeof(Driver).Name,
                    driverDto,
                    stateManager
                ),
                stateManager
            );

            var rideDto = new RideDTO();
            RideDataService = new RideDataService(
                rideStorageWrapper,
                rideDto,
                new DataImplementations.Synchronizer<Ride, Models.Ride.Ride>(
                    rideStorageWrapper,
                    typeof(Ride).Name,
                    rideDto,
                    stateManager
                ),
                stateManager
            );
        }

        public async Task SyncAzureTablesWithDict()
        {
            await AuthDataService.SyncAzureTablesWithDict();
            await DriverDataService.SyncAzureTablesWithDict();
            await RideDataService.SyncAzureTablesWithDict();
        }
        public async Task SyncDictWithAzureTable()
        {
            await AuthDataService.SyncDictWithAzureTable();
            await DriverDataService.SyncDictWithAzureTable();
            await RideDataService.SyncDictWithAzureTable();
        }
    }
}