using Contracts.Logic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Models.Auth;

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
        public IActionResult Get()
        {
            return Ok();
        }

        // POST api/<AuthController>
        [HttpPost]
        public async Task<LoginData> Post([FromBody] LoginData loginData)
        {
            return await authService.Login(loginData);
        }

        // POST api/<AuthController>/register
        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Post([FromBody] UserProfile userProfile)
        {
            var res = await authService.Register(userProfile);
            if (res)
            {
                return new ObjectResult(res) { StatusCode = StatusCodes.Status201Created };
            }

            return new ObjectResult(res) { StatusCode = StatusCodes.Status400BadRequest };
        }
    }
}
