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
        private readonly ITournamentService _tournamentService;
        
        public UserController(IUserService userService, ITournamentService tournamentService)
        {
            _userService = userService;
            _tournamentService = tournamentService;
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

        [HttpGet("{id}/tournaments")]
        public async Task<IActionResult> GetUserTournaments(string id)
        {
            var tournaments = await _tournamentService.GetTournamentsByUserAsync(id);
            
            // Map to simplified DTO to avoid Cycle in JSON (User -> Tournament -> User ...)
            var response = tournaments.Select(t => new 
            {
                t.Id,
                t.Name,
                t.Status,
                OrganizerId = t.OrganizerId,
                OrganizerName = t.Organizer?.Username ?? "Unknown",
                t.StartDate,
                t.EndDate,
                t.City,
                t.CountryCode,
                t.GameName,
                t.SystemType,
                t.PlayerLimit,
                t.Emblem
                // t.Participants // Don't include participants to save bandwidth/complexity unless needed
            });

            return Ok(response);
        }
    }
}