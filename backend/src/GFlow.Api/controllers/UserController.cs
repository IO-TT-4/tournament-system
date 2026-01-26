using Microsoft.AspNetCore.Mvc;

using GFlow.Application.DTOs;
using GFlow.Application.Ports;

using GFlow.Domain.Entities;
using System.Threading.Tasks;

namespace GFlow.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {

        private readonly IUserService _userService;
        
        public UserController(IUserService userService)
        {
            _userService = userService;
        }


        [HttpGet("{reqId}")]
        public async Task<IActionResult> Get(string reqId)
        {
            User? user = await _userService.GetUser(reqId);

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

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var users = await _userService.GetAllUsers();
            return Ok(users);
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string query)
        {
            var users = await _userService.SearchUsers(query);
            return Ok(users.Select(u => new { u.Id, u.Username, u.Email }));
        }   

    }
}