using GFlow.Domain.Entities;
using GFlow.Domain.ValueObjects;

namespace GFlow.Domain.Services.TieBreakers
{
    public interface ITieBreaker
    {
        string Name { get; }
        double Calculate(string userId, List<Match> allMatches, List<StandingsEntry> currentStandings);
    }
}