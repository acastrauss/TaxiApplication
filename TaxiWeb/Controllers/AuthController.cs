using Contracts.Logic;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Models.Auth;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TaxiWeb.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TaxiWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService authService;
        private readonly IOptions<JWTConfig> jwtConfig;

        public AuthController(IAuthService authService, IOptions<JWTConfig> jwtConfig)
        {
            this.authService = authService;
            this.jwtConfig = jwtConfig;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Get()
        {
            return Ok();
        }
        
        [HttpGet]
        [Route("jwt-check")]
        [Authorize]
        public async Task<IActionResult> CheckJwt()
        {
            var userEmail = HttpContext.User.Claims.FirstOrDefault((c) => c.Type == ClaimTypes.Email);
            var userType = HttpContext.User.Claims.FirstOrDefault((c) => c.Type == ClaimTypes.Role);
            var token = await HttpContext.GetTokenAsync("access_token");
            return Ok();
        }


        // POST api/<AuthController>/register
        [HttpPost]
        [AllowAnonymous]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody] UserProfile userProfile)
        {
            var res = await authService.Register(userProfile);
            if (res)
            {
                return new ObjectResult(res) { StatusCode = StatusCodes.Status201Created };
            }

            return new ObjectResult(res) { StatusCode = StatusCodes.Status400BadRequest };
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginData loginData)
        {
            var userExists = await authService.Login(loginData);

            if (!userExists.Item1) 
            {
                return BadRequest();
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            // TO DO: Add to config
            var key = Encoding.ASCII.GetBytes(jwtConfig.Value.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Email, loginData.Email), 
                    new Claim(ClaimTypes.Role, userExists.Item2.ToString()) 
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Audience=jwtConfig.Value.Audience,
                Issuer=jwtConfig.Value.Issuer
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            return Ok(new { Token = tokenString });
        }
    }
}
