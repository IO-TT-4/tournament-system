using Microsoft.AspNetCore.Mvc;
using GFlow.Domain.Entities;
using GFlow.Application.DTOs;
using GFlow.Application.Ports;
using Microsoft.AspNetCore.Authorization;

namespace GFlow.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TournamentController : ControllerBase
    {
        private readonly ITournamentService _tournamentService;

        public TournamentController(ITournamentService tournamentService)
        {
            _tournamentService = tournamentService;    
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<TournamentResponse>> Create([FromBody] CreateTournamentRequest request)
        {
            var tournament = await _tournamentService.CreateTournamentAsync(request);

            if (tournament is null)
            {
                // Jeśli serwis nie stworzył turnieju (np. błąd walidacji wewnątrz)
                return BadRequest("Could not create tournament.");
            }

            var response = MapToResponse(tournament);

            // CreatedAtAction jest lepsze - zwraca nagłówek Location do nowego zasobu
            return CreatedAtAction(nameof(Get), new { id = tournament.Id }, response);
        }

        [HttpGet("{id}")] // Ograniczenie do formatu Guid
        public async Task<ActionResult<TournamentResponse>> Get(string id)
        {
            var tournament = await _tournamentService.GetTournament(id);

            if (tournament is null)
            {
                return NotFound($"Tournament with ID {id} not found.");
            }
            
            return Ok(MapToResponse(tournament));
        }

        [HttpGet("upcoming")]
        public async Task<ActionResult<IEnumerable<TournamentResponse>>> GetUpcoming()
        {
            var tournaments = await _tournamentService.GetUpcomingTournaments();
            var response = tournaments.Select(MapToResponse);
            return Ok(response);
        }

        [HttpGet("current")]
        public async Task<ActionResult<IEnumerable<TournamentResponse>>> GetCurrent()
        {
            var tournaments = await _tournamentService.GetCurrentTournaments();
            var response = tournaments.Select(MapToResponse);
            return Ok(response);
        }

        // Pomocnicza metoda mapująca, aby nie powtarzać kodu (DRY)
        private static TournamentResponse MapToResponse(Tournament t)
        {
            return new TournamentResponse
            {
                Id = t.Id,
                Name = t.Name,
                OrganizerName = t.Organizer?.Username ?? "Unknown",
                PlayerLimit = t.PlayerLimit,
                StartDate = t.StartDate,
                EndDate = t.EndDate
                // Tutaj możesz dodać status turnieju lub liczbę zapisanych osób
            };
        }
    }
}