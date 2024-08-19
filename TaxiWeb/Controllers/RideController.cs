using Contracts.Logic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models.Auth;
using Models.Ride;
using System.Security.Claims;
using TaxiWeb.Services;

namespace TaxiWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RideController : ControllerBase
    {
        private readonly IBussinesLogic authService;
        private readonly IRequestAuth requestAuth;
        public RideController(IBussinesLogic authService, IRequestAuth requestAuth)
        {
            this.authService = authService;
            this.requestAuth = requestAuth;
        }

        [HttpPost]
        [Authorize]
        [Route("estimate-ride")]
        public async Task<IActionResult> EstimateRide([FromBody] EstimateRideRequest request)
        {
            var userHasRightToAccess = requestAuth.DoesUserHaveRightsToAccessResource(HttpContext, new UserType[] { UserType.CLIENT });
            if(!userHasRightToAccess)
            {
                return Unauthorized();
            }

            return Ok(await authService.EstimateRide(request));
        }

        [HttpPost]
        [Authorize]
        [Route("create-ride")]
        public async Task<IActionResult> CreateRide([FromBody] CreateRideRequest request)
        {
            var userHasRightToAccess = requestAuth.DoesUserHaveRightsToAccessResource(HttpContext, new UserType[] { UserType.CLIENT });
            if (!userHasRightToAccess)
            {
                return Unauthorized();
            }

            var userEmail = requestAuth.GetUserEmailFromContext(HttpContext);

            if (userEmail == null)
            {
                return BadRequest("Invalid JWT");
            }

            var res = await authService.CreateRide(request, userEmail);

            if (res == null) 
            {
                return BadRequest("Failed to create ride");
            }

            return Ok(res);
        }

        [HttpPatch]
        [Authorize]
        [Route("update-ride-status")]
        public async Task<IActionResult> UpdateRideStatus([FromBody] UpdateRideRequest request)
        {
            var userHasRightToAccess = requestAuth.DoesUserHaveRightsToAccessResource(HttpContext, new UserType[] { UserType.CLIENT, UserType.DRIVER });

            // Client will update once ride is finished, Driver will update when accepting the ride
            if (!userHasRightToAccess)
            {
                return Unauthorized();
            }

            var userEmail = requestAuth.GetUserEmailFromContext(HttpContext);
            var userType = requestAuth.GetUserTypeFromContext(HttpContext);

            if (userEmail == null || userType == null) 
            {
                return BadRequest("Invalid JWT");
            }

            var validUpdate = 
                (userType == UserType.CLIENT && request.Status == RideStatus.COMPLETED) 
                || (userType == UserType.DRIVER && request.Status == RideStatus.ACCEPTED);

            if (!validUpdate)
            {
                return Unauthorized("Can not update ride with given parameters");
            }

            if(userType == UserType.DRIVER)
            {
                var driverStatus = await authService.GetDriverStatus(userEmail);
                if (driverStatus != Models.UserTypes.DriverStatus.VERIFIED)
                {
                    return Unauthorized($"This driver can not accept rides as he is {driverStatus}");
                }
            }

            return Ok(await authService.UpdateRide(request, userEmail));
        }

        [HttpGet]
        [Authorize]
        [Route("get-new-rides")]
        public async Task<IActionResult> GetNewRides()
        {
            var userHasRightToAccess = requestAuth.DoesUserHaveRightsToAccessResource(HttpContext, new UserType[] { UserType.DRIVER });
            if (!userHasRightToAccess)
            {
                return Unauthorized();
            }
            
            var userEmail = requestAuth.GetUserEmailFromContext(HttpContext);
            if (userEmail == null)
            {
                return Unauthorized("Bad user email");
            }

            var driverStatus = await authService.GetDriverStatus(userEmail);

            if(driverStatus != Models.UserTypes.DriverStatus.VERIFIED)
            {
                return Unauthorized($"This driver can not see new rides as he is {driverStatus}");
            }

            return Ok(await authService.GetNewRides());
        }

        [HttpGet]
        [Authorize]
        [Route("get-user-rides")]
        public async Task<IActionResult> GetUserRides()
        {
            var userEmail = requestAuth.GetUserEmailFromContext(HttpContext);
            var userType = requestAuth.GetUserTypeFromContext(HttpContext);

            if (userEmail == null || userType == null)
            {
                return BadRequest("Invalid JWT");
            }

            return Ok(await authService.GetUsersRides(userEmail, (UserType)userType));
        }

        [HttpPost]
        [Authorize]
        [Route("get-ride")]
        public async Task<IActionResult> GetRideStatus([FromBody] GetRideStatusRequest getRideStatusRequest)
        {
            var userEmail = requestAuth.GetUserEmailFromContext(HttpContext);
            var userType = requestAuth.GetUserTypeFromContext(HttpContext);

            if (userEmail == null || userType == null)
            {
                return BadRequest("Invalid JWT");
            }

            var ride = await authService.GetRideStatus(getRideStatusRequest.ClientEmail, getRideStatusRequest.RideCreatedAtTimestamp);

            if(ride == null)
            {
                return BadRequest("Failed to get ride with those parameters");
            }

            var userIsDriverForRide = userType == UserType.DRIVER && userEmail.Equals(ride.DriverEmail);
            var userIsClientForRide = userType == UserType.CLIENT && userEmail.Equals(ride.ClientEmail);
            var userIsAdmin = userType == UserType.ADMIN;

            if(!userIsClientForRide && !userIsDriverForRide && !userIsAdmin)
            {
                return Unauthorized("You can not see this ride.");
            }

            return Ok(ride);
        }
    }
}
