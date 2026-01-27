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
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            // Force the OrganizerId to be the authenticated user
            request.OrganizerId = userId;

            var tournament = await _tournamentService.CreateTournamentAsync(request);

            if (tournament is null)
            {
                // If the service did not create the tournament (e.g. internal validation error)
                // It could also be that the user doesn't exist in the DB even if they have a token (rare but possible)
                return BadRequest("Could not create tournament. Ensure valid data.");
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
            
            var response = MapToResponse(tournament);

            
            // Enrich participants with Status from Service
            // (Previously used Standings, but that only shows active/withdrawn players in ranking, not pending/waitlist)
            var details = await _tournamentService.GetParticipantsDetailsAsync(id);
            if (details.Any())
            {
                response.Participants = details;
                response.ParticipantCount = details.Count(p => p.Status == "Confirmed"); // Correct count for UI limit display
            }
            
            return Ok(response);
        }

        [HttpPost("{id}/join")]
        [Authorize]
        public async Task<ActionResult> Join(string id)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            var result = await _tournamentService.JoinTournamentAsync(id, userId);
            
            if (result == "NotFound") return NotFound("Tournament not found.");
            if (result == "AlreadyJoined") return BadRequest("You are already joined or have a pending request.");
            
            return Ok(new { Status = result }); // "Joined", "Pending", "Waitlist"
        }

        [HttpPost("{id}/participants/{userId}/approve")]
        [Authorize]
        public async Task<ActionResult> ApproveParticipant(string id, string userId)
        {
             // Permission check
            var currentUser = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (currentUser == null) return Unauthorized();

            var tournament = await _tournamentService.GetTournament(id);
            if (tournament == null) return NotFound();
            if (tournament.OrganizerId != currentUser) return Forbid();

            var success = await _tournamentService.ProcessJoinRequestAsync(id, userId, true, currentUser);
            if (!success) return BadRequest("Could not approve. Tournament might be full.");
            
            return Ok();
        }

        [HttpPost("{id}/participants/{userId}/reject")]
        [Authorize]
        public async Task<ActionResult> RejectParticipant(string id, string userId)
        {
             // Permission check
            var currentUser = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (currentUser == null) return Unauthorized();

            var tournament = await _tournamentService.GetTournament(id);
            if (tournament == null) return NotFound();
            if (tournament.OrganizerId != currentUser) return Forbid();

            var success = await _tournamentService.ProcessJoinRequestAsync(id, userId, false, currentUser);
            if (!success) return BadRequest("Could not reject (not found?)."); // Should usually succeed
            
            return Ok();
        }

        [HttpGet]
        public async Task<ActionResult<TournamentListResponse>> GetFiltered([FromQuery] TournamentFilterParams filterParams)
        {
            // ... existing GetFiltered code ...
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

            var response = MapToResponse(tournament);
            
            // Enrich participants
            // Enrich participants
            var details = await _tournamentService.GetParticipantsDetailsAsync(id);
            if (details.Any())
            {
                response.Participants = details;
                response.ParticipantCount = details.Count(p => p.Status == "Confirmed");
            }
            
            return Ok(response);
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
            var currentUser = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var success = await _tournamentService.WithdrawParticipantAsync(id, userId, currentUser);
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
            var requestingUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (requestingUserId == null) return Unauthorized();

            var success = await _tournamentService.AddModeratorAsync(id, userId, requestingUserId);
            if (!success)
            {
                // Could be not found or forbidden (if not organizer)
                return BadRequest("Could not add moderator. Check permissions (Organizer only) or user validity.");
            }
            return NoContent();
        }

        [HttpDelete("{id}/moderators/{userId}")]
        [Authorize]
        public async Task<ActionResult> RemoveModerator(string id, string userId)
        {
            var requestingUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (requestingUserId == null) return Unauthorized();

            var success = await _tournamentService.RemoveModeratorAsync(id, userId, requestingUserId);
            if (!success)
            {
                return BadRequest("Could not remove moderator. Check permissions.");
            }
            return NoContent();
        }

        [HttpPost("{id}/rounds/start")]
        [Authorize]
        public async Task<ActionResult> StartNextRound(string id)
        {
            // Check permissions: Organizer or Moderator
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            var tournament = await _tournamentService.GetTournament(id);
            if (tournament == null) return NotFound("Tournament not found.");

            bool isOrganizer = tournament.OrganizerId == userId;
            bool isModerator = tournament.Moderators?.Any(u => u.Id == userId) ?? false;

            if (!isOrganizer && !isModerator)
            {
                return Forbid();
            }

            var success = await _tournamentService.StartNextRoundAsync(id, userId);
            if (!success)
            {
                return BadRequest("Could not start next round. Ensure previous round is finished.");
            }
            return Ok();
        }

        [HttpGet("{id}/matches")]
        public async Task<ActionResult<List<MatchDto>>> GetMatches(string id)
        {
            var matches = await _tournamentService.GetMatchesAsync(id);
            return Ok(matches);
        }

        [HttpPost("{id}/participants")]
        [Authorize]
        public async Task<ActionResult> AddParticipant(string id, [FromBody] AddParticipantDto request)
        {
             // Permission check: Organizer only (or Moderator?)
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            var tournament = await _tournamentService.GetTournament(id);
            if (tournament == null) return NotFound("Tournament not found.");
            
            if (tournament.OrganizerId != userId) // Strict: Only Organizer can manually add?
            {
                // return Forbid(); // Let's allow simple auth for now or keep strict
                // User requirement: "organizer mogl dopisac"
                if (tournament.OrganizerId != userId) return Forbid();
            }

            var success = await _tournamentService.AddParticipantAsync(id, request.Username, userId);
            if (!success) return BadRequest("Could not add participant. User might not exist or tournament is full.");

            return Ok();
        }

        [HttpDelete("{id}/participants/{userId}")]
        [Authorize]
        public async Task<ActionResult> RemoveParticipant(string id, string userId)
        {
             // Permission check
            var currentUser = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (currentUser == null) return Unauthorized();

            var tournament = await _tournamentService.GetTournament(id);
            if (tournament == null) return NotFound("Tournament not found.");
            
            if (tournament.OrganizerId != currentUser) return Forbid();

            var success = await _tournamentService.RemoveParticipantAsync(id, userId, currentUser);
            if (!success) return NotFound("Could not remove participant.");

            return NoContent();
        }

        [HttpPost("matches/{matchId}/result")]
        [Authorize]
        public async Task<ActionResult> SubmitMatchResult(string matchId, [FromBody] MatchResultDto result)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            // We do not pass TournamentId here because MatchId is unique globaly (in theory) or service handles it.
            // Service `SubmitMatchResultAsync` takes matchId.
            // Wait, route is `api/Tournament/matches/...` or `api/Tournament/{id}/matches/...`?
            // The method in service signatures: `SubmitMatchResultAsync(string matchId, ...)`
            // I'll put it at `[HttpPost("matches/{matchId}/result")]` under `api/Tournament` controller.
            // It acts as a global match update or we could nest it if we want to enforce tournament boundaries in URL. 
            // Stick to simple Global Match ID approach as Service handles validation.

            var success = await _tournamentService.SubmitMatchResultAsync(matchId, result.ScoreA, result.ScoreB, userId, result.FinishType);
            
            if (!success)
            {
                // Could be auth error or match not found or permissions
                return BadRequest("Could not submit result. Check permissions or match status.");
            }

            return Ok();
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
                NumberOfRounds = t.NumberOfRounds,
                TieBreakers = t.TieBreakers,
                GameCode = t.GameCode,
                GameName = t.GameName,
                City = t.City,
                Address = t.Address,
                Lat = t.Lat,
                Lng = t.Lng,
                ViewCount = t.ViewCount,
                ParticipantCount = t.Participants?.Count ?? 0,
                Emblem = t.Emblem,
                SystemType = t.SystemType.ToString(),
                Description = t.Description,
                OrganizerId = t.OrganizerId, // Assuming OrganizerId is on Tournament entity
                ModeratorIds = t.Moderators?.Select(m => m.Id).ToList() ?? new List<string>(),
                Participants = t.Participants?.Select(p => new ParticipantDto 
                { 
                    Id = p.Id, 
                    Username = p.Username 
                }).ToList() ?? new List<ParticipantDto>(),
                
                // Scoring
                WinPoints = t.WinPoints,
                DrawPoints = t.DrawPoints,
                LossPoints = t.LossPoints,
                RegistrationMode = t.RegistrationMode.ToString(),
                EnableMatchEvents = t.EnableMatchEvents
            };
        }

        [HttpGet("{id}/audit")]
        [Authorize]
        public async Task<ActionResult> GetAuditLogs(string id)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            var tournament = await _tournamentService.GetTournament(id);
            if (tournament == null) return NotFound("Tournament not found.");

            // Only Organizer or Moderator can view audit logs
            bool isOrganizer = tournament.OrganizerId == userId;
            bool isModerator = tournament.Moderators?.Any(u => u.Id == userId) ?? false;

            if (!isOrganizer && !isModerator)
            {
                return Forbid();
            }

            // Get both match result audits and general tournament audits
            var matchAudits = await _tournamentService.GetAuditLogsAsync(id);
            var tournamentAudits = await _tournamentService.GetTournamentAuditLogsAsync(id);
            
            // Map match audits
            var matchAuditDtos = matchAudits.Select(a => new AuditLogDto
            {
                Id = a.Id,
                MatchId = a.MatchId,
                OldScoreA = a.OldScoreA,
                OldScoreB = a.OldScoreB,
                NewScoreA = a.NewScoreA,
                NewScoreB = a.NewScoreB,
                ModifiedBy = a.ModifiedByDefaultId,
                ModifiedAt = a.ModifiedAt,
                ChangeType = a.ChangeType
            }).ToList();
            
            // Map tournament audits
            var tournamentAuditDtos = tournamentAudits.Select(a => new TournamentAuditLogDto
            {
                Id = a.Id,
                TournamentId = a.TournamentId,
                ActionType = a.ActionType,
                TargetUserId = a.TargetUserId,
                TargetUsername = a.TargetUsername,
                PerformedById = a.PerformedById,
                PerformedByUsername = a.PerformedByUsername,
                Details = a.Details,
                Timestamp = a.Timestamp
            }).ToList();

            return Ok(new 
            { 
                MatchAudits = matchAuditDtos, 
                TournamentAudits = tournamentAuditDtos 
            });
        }


    }
}