using GFlow.Application.DTOs;
using GFlow.Domain.Entities;

namespace GFlow.Application.Ports
{
    public interface ITournamentService
    {
        public Task<Tournament?> GetTournament(string id);
        public Task<List<Tournament>> GetCurrentTournaments();
        public Task<List<Tournament>> GetUpcomingTournaments();
        public Task<(List<Tournament> Data, int TotalCount)> GetTournamentsAsync(TournamentFilterParams filterParams);
        public Task<bool> TrackActivityAsync(string tournamentId, string? userId, string activityType);

        public Task<Tournament?> CreateTournamentAsync(CreateTournamentRequest request);
        public Task<Tournament?> UpdateTournamentAsync(string id, UpdateTournamentRequest request);
        public Task<bool> DeleteTournamentAsync(string id);
        
        public Task<bool> WithdrawParticipantAsync(string tournamentId, string userId);
        public Task<bool> SubmitMatchResultAsync(string matchId, double scoreA, double scoreB, string? requestingUserId = null);
        public Task<List<StandingsEntry>> GetStandingsAsync(string tournamentId);
        
        public Task<bool> AddModeratorAsync(string tournamentId, string userId);
    }
}