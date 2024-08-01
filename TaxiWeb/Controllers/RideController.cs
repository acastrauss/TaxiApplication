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
        private readonly IAuthService authService;
        public RideController(IAuthService authService)
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

        [HttpGet]
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

            var res = await authService.CreateRide(request);

            if (res == null) 
            {
                return BadRequest("Failed to create ride");
            }

            return Ok(res);
        }

        [HttpPatch]
        [Authorize]
        [Route("update-ride-status")]
        public async Task<IActionResult> AcceptRide([FromBody] UpdateRideRequest request)
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

            var validUpdate = (userType == UserType.CLIENT && request.Status == RideStatus.COMPLETED) || (userType == UserType.DRIVER && request.Status == RideStatus.ACCEPTED);

            if (!validUpdate)
            {
                return Unauthorized("Can not update ride with given parameters");
            }

            if (request.Status == RideStatus.ACCEPTED)
            {
                request.DriverEmail = userEmailClaim.Value;
            }

            return Ok(await authService.UpdateRide(request));
        }
    }
}
