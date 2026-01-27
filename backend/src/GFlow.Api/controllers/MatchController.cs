using Microsoft.AspNetCore.Mvc;
using GFlow.Application.DTOs;
using GFlow.Application.Ports;
using GFlow.Application.Services;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;

namespace GFlow.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MatchController : ControllerBase
    {
        private readonly ITournamentService _tournamentService;
        private readonly MatchEventService _eventService;

        public MatchController(ITournamentService tournamentService, MatchEventService eventService)
        {
            _tournamentService = tournamentService;
            _eventService = eventService;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<MatchDetailsDto>> GetMatch(string id)
        {
            var match = await _tournamentService.GetMatchDetailsAsync(id);
            if (match == null)
            {
                return NotFound($"Match with ID {id} not found.");
            }
            return Ok(match);
        }

        [HttpPost("{id}/result")]
        [Authorize]
        public async Task<ActionResult> SubmitResult(string id, [FromBody] SubmitMatchResultRequest request)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var success = await _tournamentService.SubmitMatchResultAsync(id, request.ScoreA, request.ScoreB, userId);
            
            if (!success)
            {
                return NotFound($"Match with ID {id} not found or permission denied.");
            }

            return Ok();
        }

        [HttpPost("{id}/events")]
        [Authorize]
        public async Task<ActionResult<MatchEventResponse>> AddEvent(string id, [FromBody] CreateMatchEventRequest request)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized();
            }

            var matchEvent = await _eventService.AddEventAsync(id, request, userId);
            if (matchEvent == null)
            {
                return NotFound("Match not found or permission denied.");
            }

            var response = new MatchEventResponse
            {
                Id = matchEvent.Id,
                MatchId = matchEvent.MatchId,
                EventType = matchEvent.EventType,
                Timestamp = matchEvent.Timestamp,
                MinuteOfPlay = matchEvent.MinuteOfPlay,
                PlayerId = matchEvent.PlayerId,
                Description = matchEvent.Description,
                Metadata = matchEvent.Metadata,
                RecordedBy = matchEvent.RecordedBy
            };

            return CreatedAtAction(nameof(GetEvents), new { id = matchEvent.MatchId }, response);
        }

        [HttpGet("{id}/events")]
        public async Task<ActionResult<List<MatchEventResponse>>> GetEvents(string id)
        {
            var events = await _eventService.GetMatchEventsAsync(id);
            return Ok(events);
        }
    }
}
