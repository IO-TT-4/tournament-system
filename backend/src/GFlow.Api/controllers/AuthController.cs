using Microsoft.AspNetCore.Mvc;

namespace GFlow.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            return Ok(new
            {
                accessToken = "abc",
                username = $"{request.username}",
                email = $"{request.username}",
                id = 1
            });
        }
    }

    public class LoginRequest
    {
        public string username {get; set;}
        public string password {get; set;}
    }
}