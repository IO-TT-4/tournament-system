using GFlow.Domain.Entities;
using GFlow.Domain.ValueObjects;

namespace GFlow.Domain.Services.TieBreakers
{
    public class RandomTieBreaker : ITieBreaker
    {
        public string Name => "Random Draw";
        public double Calculate(string userId, List<Match> allMatches, List<StandingsEntry> currentStandings)
        {
            // Use HashCode from ID so the result is consistent for the same user at a given moment
            return userId.GetHashCode();
        }
    }
}