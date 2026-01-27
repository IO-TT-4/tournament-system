using GFlow.Domain.Entities;
using GFlow.Domain.ValueObjects;
using GFlow.Application.DTOs;


namespace GFlow.Application.Ports
{
    public interface ITournamentRepository
    {
        public Task<Tournament?> GetTournament(string id);
        public Task<List<Tournament>> GetAll();
        public Task<List<Tournament>> GetCurrent();
        public Task<List<Tournament>> GetUpcoming();
        public Task<Tournament?> Add(Tournament tournament);
        public Task<Tournament?> Update(Tournament tournament);
        public Task<bool> Delete(string id);
        
        public Task<(List<Tournament> Data, int TotalCount)> GetFiltered(TournamentFilterParams filterParams);
        public Task AddActivity(UserActivity activity);
        public Task IncrementViewCount(string tournamentId);
        public Task<List<UserActivity>> GetUserActivities(string userId, int limit = 50);


        public Task<Match?> GetMatchById(string matchId);



        public Task<List<Match>> GetMatchesByRound(string tournamentId, int roundNumber);
    
        public Task<List<TournamentParticipant>> GetParticipants(string tournamentId);
    
        public Task<List<Match>> GetAllMatches(string tournamentId);

        public Task SaveMatches(IEnumerable<Match> matches);

        public Task<bool> UpdateParticipant(TournamentParticipant participant);
        public Task AddParticipant(TournamentParticipant participant);
        public Task<bool> DeleteParticipant(string tournamentId, string userId);
        public Task<TournamentParticipant?> GetParticipant(string tournamentId, string userId);
        
        // Changing to Task for proper await capability
        public Task UpdateMatch(Match match);
        public Task AddMatchResultAudit(MatchResultAudit audit);
        public Task<List<MatchResultAudit>> GetMatchResultAudits(string tournamentId);
        
        // General tournament audit logs
        public Task AddTournamentAuditAsync(TournamentAuditLog log);
        public Task<List<TournamentAuditLog>> GetTournamentAuditsAsync(string tournamentId);
        
        public Task<List<Tournament>> GetTournamentsByUserId(string userId);
    }
}