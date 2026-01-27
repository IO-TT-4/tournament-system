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
        
        public Task<bool> WithdrawParticipantAsync(string tournamentId, string userId, string? performedById = null);
        public Task<bool> SubmitMatchResultAsync(string matchId, double scoreA, double scoreB, string? requestingUserId = null, string finishType = "Normal");
        public Task<List<StandingsEntry>> GetStandingsAsync(string tournamentId);
        
        public Task<bool> AddModeratorAsync(string tournamentId, string userId, string? requestingUserId = null);
        public Task<bool> RemoveModeratorAsync(string tournamentId, string userId, string? requestingUserId = null);
        public Task<bool> StartNextRoundAsync(string tournamentId, string? performedById = null);
        public Task<List<MatchDto>> GetMatchesAsync(string tournamentId);
        public Task<MatchDetailsDto?> GetMatchDetailsAsync(string matchId);
        public Task<bool> AddParticipantAsync(string tournamentId, string username, string? performedById = null);
        public Task<bool> RemoveParticipantAsync(string tournamentId, string userId, string? performedById = null);
        
        // Registration
        public Task<string> JoinTournamentAsync(string tournamentId, string userId); // Returns status message ("Joined", "Waitlist", "Pending")
        public Task<bool> ProcessJoinRequestAsync(string tournamentId, string userId, bool isApproved, string? performedById = null);
        public Task<List<ParticipantDto>> GetParticipantsDetailsAsync(string tournamentId); // Full details including status
        
        // Audit
        public Task<List<MatchResultAudit>> GetAuditLogsAsync(string tournamentId);
        public Task<List<TournamentAuditLog>> GetTournamentAuditLogsAsync(string tournamentId);
        
        public Task<List<Tournament>> GetTournamentsByUserAsync(string userId);
    }
}