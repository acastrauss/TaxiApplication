using Models.Auth;
using System.Security.Claims;

namespace TaxiWeb.Services
{
    public class RequestAuth : IRequestAuth
    {
        public bool DoesUserHaveRightsToAccessResource(HttpContext httpContext, UserType[] allowedUserTypes)
        {
            var userEmailClaim = httpContext.User.Claims.FirstOrDefault((c) => c.Type == ClaimTypes.Email);
            var userTypeClaim = httpContext.User.Claims.FirstOrDefault((c) => c.Type == ClaimTypes.Role);

            if (userEmailClaim == null || userTypeClaim == null)
            {
                return false;
            }

            var isParsed = Enum.TryParse(userTypeClaim.Value, out UserType userType);

            if (!isParsed)
            {
                return false;
            }

            if (!allowedUserTypes.Contains(userType))
            {
                return false;
            }

            return true;
        }
    }
}
