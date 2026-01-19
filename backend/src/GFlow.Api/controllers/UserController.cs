using Microsoft.AspNetCore.Mvc;

using GFlow.Application.DTOs;
using GFlow.Application.Ports;

using GFlow.Domain.Entities;

namespace GFlow.Api.Controllers
{
    [ApiController]
    [Route("api")]
    public class UserController : ControllerBase
    {

        private readonly IUserService _userService;
        
        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            User? user =_userService.Login(request);

            if(user is null)
            {
                return Unauthorized(new
                {
                    code = "INVALID_CREDENTIALS"
                });
            }

            return Ok(new
            {
                id=user.Id,
                username=user.Username,
                email=user.Email,
                token=user.Token
            });
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterRequest request)
        {
            User? user =_userService.RegisterUser(request);

            if(user is null)
            {
                return BadRequest();
            }

            return Ok(new
            {
                id=user.Id,
                username=user.Username,
                email=user.Email,
                token=user.Token
            });
        }

        [HttpGet("{reqId}")]
        public IActionResult Get(string reqId)
        {
            User? user =_userService.GetUser(reqId);

            if(user is null)
            {
                return BadRequest();
            }

            return Ok(new
            {
                id=user.Id,
                username=user.Username,
                email=user.Email,
            });
        }

    }
}