using System.Reflection;
using System.Threading.Tasks;
using GFlow.Application.DTOs;
using GFlow.Application.Ports;
using GFlow.Domain.Entities;

namespace GFlow.Application.Services
{
    public class TournamentService : ITournamentService
    {
        public async Task<Tournament?> CreateTournamentAsync(CreateTournamentRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name) || request.Name.Length < 5)
            {
                return null;
            }

            if (request.MaxParticipants <= 0)
            {
                return null;
            }

            var tournament = new Tournament
            {
                Id = Guid.NewGuid().ToString(),
                Name = request.Name,
                OrganizerId = request.OrganizerId,
                SystemType = request.SystemType,
                PlayerLimit = request.MaxParticipants, 
                Status = Domain.ValueObjects.TournamentStatus.CREATED,
                StartDate = request.StartDate,
                EndDate = request.EndDate
            };
            

            return await _tournamentRepo.Add(tournament);; 
        }
        
        private readonly ITournamentRepository _tournamentRepo;

        public TournamentService(ITournamentRepository tournamentRepository)
        {
            _tournamentRepo = tournamentRepository;
        }


        public async Task<List<Tournament>> GetCurrentTournaments()
        {
            return await _tournamentRepo.GetCurrent();
        }

        public async Task<Tournament?> GetTournament(string id)
        {
            return await _tournamentRepo.GetTournament(id);
        }

        public async Task<List<Tournament>> GetUpcomingTournaments()
        {
            return await _tournamentRepo.GetUpcoming();
        }
    }
}