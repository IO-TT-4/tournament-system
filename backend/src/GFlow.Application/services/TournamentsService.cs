using System.Reflection;
using GFlow.Application.DTOs;
using GFlow.Application.Ports;
using GFlow.Domain.Entities;

namespace GFlow.Application.Services
{
    public class TournamentService : ITournamentService
    {
        public Tournament? CreateTournament(CreateTournamentRequest request)
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

            return _tournamentRepo.Add(tournament);
        }
        
        private readonly ITournamentRepository _tournamentRepo;

        public TournamentService(ITournamentRepository tournamentRepository)
        {
            _tournamentRepo = tournamentRepository;
        }


        public List<Tournament> GetCurrentTournaments()
        {
            return _tournamentRepo.GetCurrent();
        }

        public Tournament? GetTournament(string id)
        {
            return _tournamentRepo.Get(id);
        }

        public List<Tournament> GetUpcomingTournaments()
        {
            return _tournamentRepo.GetUpcoming();
        }
    }
}