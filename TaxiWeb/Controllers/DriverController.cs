using Contracts.Logic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Models.Auth;
using Models.UserTypes;
using System.Security.Claims;
using TaxiWeb.Models;
using static TaxiWeb.Controllers.DriverController;

namespace TaxiWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DriverController : ControllerBase
    {
        public class DriverEmail
        {
            public string Email { get; set; }
        }

        private readonly IAuthService authService;
        public DriverController(IAuthService authService)
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
        [Route("driver-status")]
        public async Task<IActionResult> GetDriverStatus([FromBody] DriverEmail driverEmail)
        {
            if (!DoesUserHasRightsToAccess(new UserType[] { UserType.ADMIN, UserType.DRIVER }))
            {
                return Unauthorized();
            }

            var driverStatus = await authService.GetDriverStatus(driverEmail.Email);

            return Ok(driverStatus);
        }

        public class UpdateDriverStatusData
        {
            public string Email { get; set; }
            public DriverStatus Status { get; set; }
        }

        [HttpPost]
        [Authorize]
        [Route("driver-status")]
        public async Task<IActionResult> UpdateDriverStatus([FromBody] UpdateDriverStatusData updateData)
        {
            if (!DoesUserHasRightsToAccess(new UserType[] { UserType.ADMIN }))
            {
                return Unauthorized();
            }

            var result = await authService.UpdateDriverStatus(updateData.Email, updateData.Status);

            return Ok(result);
        }

        [HttpGet]
        [Authorize]
        [Route("list-drivers")]
        public async Task<IActionResult> ListAllDrivers()
        {
            if (!DoesUserHasRightsToAccess(new UserType[] { UserType.ADMIN }))
            {
                return Unauthorized();
            }

            return Ok(await authService.ListAllDrivers());
        }
    }
}
