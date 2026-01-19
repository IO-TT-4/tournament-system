using GFlow.Application.DTOs;
using GFlow.Domain.Entities;

namespace GFlow.Application.Ports
{
    public interface ITournamentService
    {
        public Task<Tournament> GetTournament(string id);
        public Task<List<Tournament>> GetCurrentTournaments();
        public Task<List<Tournament>> GetUpcomingTournaments();
        public Task<Tournament> CreateTournamentAsync(CreateTournamentRequest request);
    }
}