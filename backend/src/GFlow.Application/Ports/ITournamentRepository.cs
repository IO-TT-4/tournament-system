using System.Runtime.CompilerServices;
using GFlow.Domain.Entities;
using GFlow.Domain.ValueObjects;

namespace GFlow.Application.Ports
{
    public interface ITournamentRepository
    {
        public Task<Tournament> GetTournament(string id);
        public Task<List<Tournament>> GetAll();
        public Task<List<Tournament>> GetCurrent();
        public Task<List<Tournament>> GetUpcoming();
        public Task<Tournament> Add(Tournament tournament);
        public Task<Tournament> Update(Tournament tournament);

        public Task<Match> GetMatchById(string matchId);

        public Task<AsyncVoidMethodBuilder> UpdateMatch(Match match);

        public Task<List<Match>> GetMatchesByRound(string tournamentId, int roundNumber);
    
        public Task<List<TournamentParticipant>> GetParticipants(string tournamentId);
    
        public Task<List<Match>> GetAllMatches(string tournamentId);

        public Task<AsyncVoidMethodBuilder> SaveMatches(IEnumerable<Match> matches);
    }
}