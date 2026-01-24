using GFlow.Domain.Entities;

namespace GFlow.Application.Ports
{
    public interface IMatchEventRepository
    {
        Task<MatchEvent> Add(MatchEvent matchEvent);
        Task<List<MatchEvent>> GetByMatch(string matchId);
    }
}
