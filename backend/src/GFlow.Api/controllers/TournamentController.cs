using Microsoft.AspNetCore.Mvc;
using GFlow.Domain.Entities;
using GFlow.Application.DTOs;
using GFlow.Application.Ports;

namespace GFlow.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TournamentController : ControllerBase
    {

        public readonly ITournamentService tournamentService;

        public TournamentController(ITournamentService tournamentService)
        {
            this.tournamentService = tournamentService;    
        }

        [HttpPost]
        public IActionResult Create(CreateTournamentRequest request)
        {
            Tournament? tournament = tournamentService.CreateTournament(request);

            if(tournament is null)
            {
                return BadRequest();
            }

            return CreatedAtAction(nameof(Get), new { id = tournament.Id }, tournament);
        }

        [HttpGet("{id}")]
        public IActionResult Get(string id)
        {
            Tournament? tournament = tournamentService.GetTournament(id);

            if(tournament is null)
            {
                return BadRequest();
            }
            
            return Ok(new
            {
               id=tournament.Id.ToString(),
               name=tournament.Name 
            });
        }

        [HttpGet("upcoming")]
        public IActionResult GetUpcoming()
        {
            var tournaments = tournamentService.GetUpcomingTournaments();

            var response = tournaments.Select(t => new TournamentResponse
            {
                Id = t.Id,
                Name = t.Name,
                OrganizerName = t.Organizer?.Username ?? "Unknown",
                PlayerLimit = t.PlayerLimit,
                StartDate = t.StartDate
            });

            return Ok(response);
        }

        [HttpGet("current")]
        public IActionResult GetCurrent()
        {
            var tournaments = tournamentService.GetCurrentTournaments();

            var response = tournaments.Select(t => new TournamentResponse
            {
                Id = t.Id,
                Name = t.Name,
                OrganizerName = t.Organizer?.Username ?? "Unknown",
                PlayerLimit = t.PlayerLimit,
                StartDate = t.StartDate
            });

            return Ok(response);
        }
    }

    
}