using Contracts.Logic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Models.Auth;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TaxiWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private IAuthService authService { get; set; }
        public AuthController(IAuthService authService)
        {
            this.authService = authService;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Get()
        {
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

            if (!userExists) 
            {
                return BadRequest();
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            // TO DO: Add to config
            var key = Encoding.ASCII.GetBytes("hnp+XZqXd9T(ev#&X4?Ng-=pm;b-MieT1@tZGQ1eM#(2PA:P0wSyG_jJbcgt$e05Zp&Pwfa;Sw}qtN/iHEz61_F}93!vX=EaF(3#");
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Email, loginData.Email), 
                    new Claim(ClaimTypes.Role, loginData.Type.ToString()) 
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            return Ok(new { Token = tokenString });
        }
    }
}
