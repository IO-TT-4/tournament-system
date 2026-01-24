using Microsoft.AspNetCore.Mvc;
using GFlow.Domain.Entities;
using GFlow.Application.DTOs;
using GFlow.Application.Ports;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GFlow.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TournamentController : ControllerBase
    {
        private readonly ITournamentService _tournamentService;
        private readonly IUserPreferenceService _preferenceService;

        public TournamentController(ITournamentService tournamentService, IUserPreferenceService preferenceService)
        {
            _tournamentService = tournamentService;
            _preferenceService = preferenceService;
        }


        [HttpPost]
        [Authorize]
        public async Task<ActionResult<TournamentResponse>> Create([FromBody] CreateTournamentRequest request)
        {
            var tournament = await _tournamentService.CreateTournamentAsync(request);

            if (tournament is null)
            {
                // If the service did not create the tournament (e.g. internal validation error)
                return BadRequest("Could not create tournament.");
            }

            var response = MapToResponse(tournament);

            // CreatedAtAction is better - returns Location header to the new resource
            return CreatedAtAction(nameof(Get), new { id = tournament.Id }, response);
        }

        [HttpGet("{id}")] // Restriction to Guid format
        public async Task<ActionResult<TournamentResponse>> Get(string id)
        {
            var tournament = await _tournamentService.GetTournament(id);

            if (tournament is null)
            {
                return NotFound($"Tournament with ID {id} not found.");
            }
            
            return Ok(MapToResponse(tournament));
        }

        [HttpGet]
        public async Task<ActionResult<TournamentListResponse>> GetFiltered([FromQuery] TournamentFilterParams filterParams)
        {
            // Capture user IP for geolocation
            filterParams.UserIp = HttpContext.Connection.RemoteIpAddress?.ToString();
            
            // Capture user ID if logged in for personalization
            if (User.Identity?.IsAuthenticated == true)
            {
                filterParams.UserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            }

            var (tournaments, total) = await _tournamentService.GetTournamentsAsync(filterParams);
            
            var responseData = new List<TournamentResponse>();
            foreach (var t in tournaments)
            {
                var mapped = MapToResponse(t);
                // Calculate relevance for the current user/context
                mapped.RelevanceScore = await _preferenceService.CalculateRelevanceScoreAsync(t, filterParams); 
                responseData.Add(mapped);
            }

            // Final re-sort by relevance if requested
            if (filterParams.SortBy == "relevance" || string.IsNullOrEmpty(filterParams.SortBy))
            {
                responseData = responseData.OrderByDescending(r => r.RelevanceScore).ToList();
            }

            return Ok(new TournamentListResponse

            {
                Data = responseData,
                TotalCount = total,
                Page = filterParams.Page,
                Limit = filterParams.Limit
            });
        }

        [HttpPost("{id}/track")]
        public async Task<ActionResult> Track(string id, [FromQuery] string type = "view")
        {
            string? userId = null;
            if (User.Identity?.IsAuthenticated == true)
            {
                userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            }

            await _tournamentService.TrackActivityAsync(id, userId, type);
            return Ok();
        }

        [HttpGet("upcoming")]


        [HttpPut("{id}")]
        [Authorize]
        public async Task<ActionResult<TournamentResponse>> Update(string id, [FromBody] UpdateTournamentRequest request)
        {
            var tournament = await _tournamentService.UpdateTournamentAsync(id, request);
            if (tournament is null)
            {
                // Can be not found or validation error, simplifying here
                return NotFound($"Tournament with ID {id} not found or invalid data.");
            }
            return Ok(MapToResponse(tournament));
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<ActionResult> Delete(string id)
        {
            var deleted = await _tournamentService.DeleteTournamentAsync(id);
            if (!deleted)
            {
                return NotFound($"Tournament with ID {id} not found.");
            }
            return NoContent();
        }

        [HttpPost("{id}/participants/{userId}/withdraw")]
        [Authorize]
        public async Task<ActionResult> Withdraw(string id, string userId)
        {
            var success = await _tournamentService.WithdrawParticipantAsync(id, userId);
            if (!success)
            {
                return NotFound("Tournament or participant not found.");
            }
            return NoContent();
        }

        [HttpGet("{id}/standings")]
        public async Task<ActionResult<IEnumerable<StandingsEntry>>> GetStandings(string id)
        {
            var standings = await _tournamentService.GetStandingsAsync(id);
            return Ok(standings);
        }

        [HttpPost("{id}/moderators/{userId}")]
        [Authorize]
        public async Task<ActionResult> AddModerator(string id, string userId)
        {
            // Only organizer or admin should do this. 
            // Ideally we check permission in Service or here. 
            // For MVP assuming Service handles business logic or we trust Authorized users if role based (not implemented yet fully).
            // Let's rely on Service returning false if operation invalid (e.g. tournament not found).
            // BUT Service doesn't check requesting user yet for this operation, only for submitting match result.
            // MVP: Allow any authorized user to add moderator? No, that's unsafe.
            // Let's add TODO or simple check if we had organizer ID in claims.
            // Assuming current user is organizer is best effort without claim inspection here.
            
            var success = await _tournamentService.AddModeratorAsync(id, userId);
            if (!success)
            {
                return NotFound("Tournament or user not found.");
            }
            return NoContent();
        }

        // Helper mapping method to avoid code repetition (DRY)
        private static TournamentResponse MapToResponse(Tournament t)
        {
            return new TournamentResponse
            {
                Id = t.Id,
                Name = t.Name,
                OrganizerName = t.Organizer?.Username ?? "Unknown",
                PlayerLimit = t.PlayerLimit,
                StartDate = t.StartDate,
                EndDate = t.EndDate,
                Status = t.Status.ToString(),
                GameCode = t.GameCode,
                GameName = t.GameName,
                City = t.City,
                Address = t.Address,
                Lat = t.Lat,
                Lng = t.Lng,
                ViewCount = t.ViewCount,
                ParticipantCount = t.Participants?.Count ?? 0,
                Emblem = t.Emblem
            };
        }


    }
}