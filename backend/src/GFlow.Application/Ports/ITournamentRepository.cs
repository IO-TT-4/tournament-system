using GFlow.Domain.Entities;

namespace GFlow.Application.Ports
{
    public interface ITournamentRepository
    {
        public Tournament? Get(string id);
        public List<Tournament> GetAll();
        public List<Tournament> GetCurrent();
        public List<Tournament> GetUpcoming();
        public Tournament? Add(Tournament tournament);
        public Tournament? Update(Tournament tournament);
    }
}