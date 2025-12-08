using Microsoft.AspNetCore.Mvc;
using GFlow.Application.Services;
using GFlow.Domain.Models;

namespace GFlow.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TournamentController : ControllerBase
    {
        [HttpGet("{reqId}")]
        public IActionResult Get(int reqId)
        {
            TournamentService aa = new TournamentService();
            Tournament tournament = aa.getTournament(reqId);
            return Ok(tournament);
        }

        [HttpGet("current")]
        public IActionResult GetCurrent(int reqId)
        {
            TournamentService aa = new TournamentService();
            List<Tournament> list = aa.GetCurrentTournaments();
            return Ok(list);
        }

        [HttpGet("upcoming")]
        public IActionResult GetUpcoming(int reqId)
        {
            TournamentService aa = new TournamentService();
            List<Tournament> list = aa.GetUpcomingTournaments();
            return Ok(list);
        }
    }

    
}