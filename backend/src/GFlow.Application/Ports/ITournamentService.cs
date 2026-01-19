using GFlow.Application.DTOs;
using GFlow.Domain.Entities;

namespace GFlow.Application.Ports
{
    public interface ITournamentService
    {
        public Tournament? GetTournament(string id);
        public List<Tournament> GetCurrentTournaments();
        public List<Tournament> GetUpcomingTournaments();
        public Tournament? CreateTournament(CreateTournamentRequest request);
    }
}