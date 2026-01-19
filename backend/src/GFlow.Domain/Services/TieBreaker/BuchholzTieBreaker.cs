using GFlow.Domain.Entities;
using GFlow.Domain.ValueObjects;

namespace GFlow.Domain.Services.TieBreakers
{
    public class BuchholzTieBreaker : ITieBreaker
    {
        public string Name => "Buchholz";

        public double Calculate(string userId, List<Match> allMatches, List<StandingsEntry> currentStandings)
        {
          var opponentIds = allMatches
            .Where(m => m.IsCompleted && (m.PlayerHomeId == userId || m.PlayerAwayId == userId))
            .Select(m => m.PlayerHomeId == userId ? m.PlayerAwayId : m.PlayerHomeId)
            .Where(id => id != Guid.Empty.ToString());

            return currentStandings.Where(s => opponentIds.Contains(s.UserId)).Sum(s => s.Points);
        }
    }
}