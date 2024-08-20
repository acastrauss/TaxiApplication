using Contracts.Logic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Models.Auth;
using Models.UserTypes;
using System.Security.Claims;
using TaxiWeb.Services;
using static TaxiWeb.Controllers.DriverController;

namespace TaxiWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DriverController : ControllerBase
    {
        private readonly IBussinesLogic authService;
        private readonly IRequestAuth requestAuth;
        public DriverController(IBussinesLogic authService, IRequestAuth requestAuth)
        {
            this.authService = authService;
            this.requestAuth = requestAuth;
        }

        [HttpPost]
        [Authorize]
        [Route("driver-status")]
        public async Task<IActionResult> GetDriverStatus([FromBody] DriverEmail driverEmail)
        {
            bool userCanAccessResource = requestAuth.DoesUserHaveRightsToAccessResource(HttpContext, new UserType[] { UserType.ADMIN, UserType.DRIVER });
            if (!userCanAccessResource)
            {
                return Unauthorized();
            }

            var driverStatus = await authService.GetDriverStatus(driverEmail.Email);

            return Ok(driverStatus);
        }

        [HttpPatch]
        // TO DO: Change to patch
        [Authorize]
        [Route("driver-status")]
        public async Task<IActionResult> UpdateDriverStatus([FromBody] UpdateDriverStatusData updateData)
        {
            bool userCanAccessResource = requestAuth.DoesUserHaveRightsToAccessResource(HttpContext, new UserType[] { UserType.ADMIN });
            if (!userCanAccessResource)
            {
                return Unauthorized();
            }

            var result = await authService.UpdateDriverStatus(updateData.Email, updateData.Status);

            if (result)
            {
                await authService.SendEmail(new Models.Email.SendEmailRequest()
                {
                    Body = $"Your status on TaxiWeb application has been changed to {updateData.Status.ToString()}",
                    // TO DO: Change to driver's email
                    EmailTo = "acastrauss@hotmail.com",
                    Subject = "TaxiWeb status update"
                });
            }

            return Ok(result);
        }

        [HttpGet]
        [Authorize]
        [Route("list-drivers")]
        public async Task<IActionResult> ListAllDrivers()
        {
            bool userCanAccessResource = requestAuth.DoesUserHaveRightsToAccessResource(HttpContext, new UserType[] { UserType.ADMIN });
            if (!userCanAccessResource)
            {
                return Unauthorized();
            }

            return Ok(await authService.ListAllDrivers());
        }

        [HttpPost]
        [Authorize]
        [Route("rate-driver")]
        public async Task<IActionResult> RateDriver([FromBody] RideRating driverRating)
        {
            bool userCanAccessResource = requestAuth.DoesUserHaveRightsToAccessResource(HttpContext, new UserType[] { UserType.CLIENT });
            if (!userCanAccessResource)
            {
                return Unauthorized();
            }

            return Ok(await authService.RateDriver(driverRating));
        }

        [HttpPost]
        [Authorize]
        [Route("avg-rating-driver")]
        public async Task<IActionResult> AverageRatingDriver([FromBody] DriverEmail driverEmail)
        {
            bool userCanAccessResource = requestAuth.DoesUserHaveRightsToAccessResource(HttpContext, new UserType[] { UserType.ADMIN });
            if (!userCanAccessResource)
            {
                return Unauthorized();
            }

            return Ok(await authService.GetAverageRatingForDriver(driverEmail.Email));
        }
    }
}
