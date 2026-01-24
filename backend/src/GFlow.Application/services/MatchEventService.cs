using GFlow.Application.DTOs;
using GFlow.Application.Ports;
using GFlow.Domain.Entities;

namespace GFlow.Application.Services
{
    public class MatchEventService
    {
        private readonly IMatchEventRepository _eventRepo;
        private readonly ITournamentRepository _tournamentRepo;
        private readonly IUserRepository _userRepo;

        public MatchEventService(
            IMatchEventRepository eventRepo,
            ITournamentRepository tournamentRepo,
            IUserRepository userRepo)
        {
            _eventRepo = eventRepo;
            _tournamentRepo = tournamentRepo;
            _userRepo = userRepo;
        }

        public async Task<MatchEvent?> AddEventAsync(string matchId, CreateMatchEventRequest request, string userId)
        {
            // Get match to verify it exists and get tournament ID
            var match = await _tournamentRepo.GetMatchById(matchId);
            if (match == null) return null;

            // Get tournament to check permissions
            var tournament = await _tournamentRepo.GetTournament(match.TournamentId);
            if (tournament == null) return null;

            // Check if user is organizer or moderator
            bool isOrganizer = tournament.OrganizerId == userId;
            bool isModerator = tournament.Moderators.Any(m => m.Id == userId);
            
            if (!isOrganizer && !isModerator)
            {
                return null; // Not authorized
            }

            // Create event
            var matchEvent = new MatchEvent(matchId, request.EventType, userId)
            {
                MinuteOfPlay = request.MinuteOfPlay,
                PlayerId = request.PlayerId,
                Description = request.Description,
                Metadata = request.Metadata
            };

            return await _eventRepo.Add(matchEvent);
        }

        public async Task<List<MatchEventResponse>> GetMatchEventsAsync(string matchId)
        {
            var events = await _eventRepo.GetByMatch(matchId);
            var responses = new List<MatchEventResponse>();

            foreach (var ev in events)
            {
                string? playerName = null;
                string? recordedByName = null;

                if (!string.IsNullOrEmpty(ev.PlayerId))
                {
                    var player = await _userRepo.Get(ev.PlayerId);
                    playerName = player?.Username;
                }

                var recorder = await _userRepo.Get(ev.RecordedBy);
                recordedByName = recorder?.Username;

                responses.Add(new MatchEventResponse
                {
                    Id = ev.Id,
                    MatchId = ev.MatchId,
                    EventType = ev.EventType,
                    Timestamp = ev.Timestamp,
                    MinuteOfPlay = ev.MinuteOfPlay,
                    PlayerId = ev.PlayerId,
                    PlayerName = playerName,
                    Description = ev.Description,
                    Metadata = ev.Metadata,
                    RecordedBy = ev.RecordedBy,
                    RecordedByName = recordedByName
                });
            }

            return responses;
        }
    }
}
