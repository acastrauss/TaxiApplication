using Contracts.Logic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models.Auth;
using Models.Ride;
using System.Security.Claims;

namespace TaxiWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RideController : ControllerBase
    {
        private readonly IBussinesLogic authService;
        public RideController(IBussinesLogic authService)
        {
            this.authService = authService;
        }

        private bool DoesUserHasRightsToAccess(UserType[] allowedTypes)
        {
            var userEmailClaim = HttpContext.User.Claims.FirstOrDefault((c) => c.Type == ClaimTypes.Email);
            var userTypeClaim = HttpContext.User.Claims.FirstOrDefault((c) => c.Type == ClaimTypes.Role);

            if (userEmailClaim == null || userTypeClaim == null)
            {
                return false;
            }

            var isParsed = Enum.TryParse(userTypeClaim.Value, out UserType userType);

            if (!isParsed)
            {
                return false;
            }

            if (!allowedTypes.Contains(userType))
            {
                return false;
            }

            return true;
        }

        [HttpPost]
        [Authorize]
        [Route("estimate-ride")]
        public async Task<IActionResult> EstimateRide([FromBody] EstimateRideRequest request)
        {
            if(!DoesUserHasRightsToAccess(new UserType[] { UserType.CLIENT }))
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
            // TO DO: Take client email from JWT
            if (!DoesUserHasRightsToAccess(new UserType[] { UserType.CLIENT }))
            {
                return Unauthorized();
            }
            var userEmailClaim = HttpContext.User.Claims.FirstOrDefault((c) => c.Type == ClaimTypes.Email);

            if (userEmailClaim == null)
            {
                return BadRequest("Invalid JWT");
            }

            var res = await authService.CreateRide(request, userEmailClaim.Value);

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
            // Client will update once ride is finished, Driver will update when accepting the ride
            if (!DoesUserHasRightsToAccess(new UserType[] { UserType.CLIENT, UserType.DRIVER }))
            {
                return Unauthorized();
            }

            var userEmailClaim = HttpContext.User.Claims.FirstOrDefault((c) => c.Type == ClaimTypes.Email);
            var userTypeClaim = HttpContext.User.Claims.FirstOrDefault((c) => c.Type == ClaimTypes.Role);

            if (userEmailClaim == null || userTypeClaim == null) 
            {
                return BadRequest("Invalid JWT");
            }

            var isParsed = Enum.TryParse(userTypeClaim.Value, out UserType userType);

            if (!isParsed)
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
                var driverStatus = await authService.GetDriverStatus(userEmailClaim.Value);
                if (driverStatus != Models.UserTypes.DriverStatus.VERIFIED)
                {
                    return Unauthorized($"This driver can not accept rides as he is {driverStatus}");
                }
            }

            return Ok(await authService.UpdateRide(request, userEmailClaim.Value));
        }

        [HttpGet]
        [Authorize]
        [Route("get-new-rides")]
        public async Task<IActionResult> GetNewRides()
        {
            if (!DoesUserHasRightsToAccess(new UserType[] { UserType.DRIVER }))
            {
                return Unauthorized();
            }
            var userEmailClaim = HttpContext.User.Claims.FirstOrDefault((c) => c.Type == ClaimTypes.Email);

            if (userEmailClaim == null) 
            {
                return Unauthorized("Bad user email");
            }

            var driverStatus = await authService.GetDriverStatus(userEmailClaim.Value);

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
            var userEmailClaim = HttpContext.User.Claims.FirstOrDefault((c) => c.Type == ClaimTypes.Email);
            var userTypeClaim = HttpContext.User.Claims.FirstOrDefault((c) => c.Type == ClaimTypes.Role);

            if (userEmailClaim == null || userTypeClaim == null)
            {
                return BadRequest("Invalid JWT");
            }

            var isParsed = Enum.TryParse(userTypeClaim.Value, out UserType userType);

            if (!isParsed)
            {
                return BadRequest("Invalid JWT");
            }

            return Ok(await authService.GetUsersRides(userEmailClaim.Value, userType));
        }

        [HttpPost]
        [Authorize]
        [Route("get-ride-status")]
        public async Task<IActionResult> GetRideStatus([FromBody] GetRideStatusRequest getRideStatusRequest)
        {
            var userEmailClaim = HttpContext.User.Claims.FirstOrDefault((c) => c.Type == ClaimTypes.Email);
            var userTypeClaim = HttpContext.User.Claims.FirstOrDefault((c) => c.Type == ClaimTypes.Role);

            if (userEmailClaim == null || userTypeClaim == null)
            {
                return BadRequest("Invalid JWT");
            }

            var isParsed = Enum.TryParse(userTypeClaim.Value, out UserType userType);

            if (!isParsed)
            {
                return BadRequest("Invalid JWT");
            }

            var ride = await authService.GetRideStatus(getRideStatusRequest.ClientEmail, getRideStatusRequest.RideCreatedAtTimestamp);

            if(ride == null)
            {
                return BadRequest("Failed to get ride with those parameters");
            }

            var userIsDriverForRide = userType == UserType.DRIVER && userEmailClaim.Value.Equals(ride.DriverEmail);
            var userIsClientForRide = userType == UserType.CLIENT && userEmailClaim.Value.Equals(ride.ClientEmail);
            var userIsAdmin = userType == UserType.ADMIN;

            if(!userIsClientForRide && !userIsDriverForRide && !userIsAdmin)
            {
                return Unauthorized("You can not see this ride.");
            }

            return Ok(ride.Status);
        }
    }
}
