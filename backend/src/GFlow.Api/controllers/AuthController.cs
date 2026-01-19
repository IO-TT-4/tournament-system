using Microsoft.AspNetCore.Mvc;
using GFlow.Application.Ports;
using GFlow.Application.DTOs;
using System.Threading.Tasks;

namespace GFlow.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {

        private readonly IAuthService _authService;
        
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            AuthResponse? authResponse = await _authService.Login(request);

            if(authResponse is null)
            {
                return Unauthorized(new
                {
                    code = "INVALID_CREDENTIALS"
                });
            }

            return Ok(new
            {
                id=authResponse.Id,
                username=authResponse.Username,
                email=authResponse.Email,
                token=authResponse.Token
            });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            AuthResponse? authResponse = await _authService.RegisterUser(request);

            if(authResponse is null)
            {
                return BadRequest();
            }

            return Ok(new
            {
                id=authResponse.Id,
                username=authResponse.Username,
                email=authResponse.Email,
                token=authResponse.Token
            });
        }
    }
}