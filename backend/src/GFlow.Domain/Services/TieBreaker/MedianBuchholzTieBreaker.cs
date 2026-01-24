using GFlow.Domain.Entities;
using GFlow.Domain.ValueObjects;

namespace GFlow.Domain.Services.TieBreakers
{
    public class MedianBuchholzTieBreaker : ITieBreaker
    {
        public string Name => "Median Buchholz";

        public double Calculate(string userId, List<Match> allMatches, List<StandingsEntry> currentStandings)
        {
            // Get opponent IDs
            var opponentIds = allMatches
                .Where(m => m.IsCompleted && (m.PlayerHomeId == userId || m.PlayerAwayId == userId))
                .Select(m => m.PlayerHomeId == userId ? m.PlayerAwayId : m.PlayerHomeId)
                .Where(id => id != Guid.Empty.ToString());

            // Get opponent scores
            var opponentScores = currentStandings
                .Where(s => opponentIds.Contains(s.UserId))
                .Select(s => s.Points)
                .OrderBy(p => p)
                .ToList();

            // If 2 or fewer opponents, return sum (no trimming possible)
            if (opponentScores.Count <= 2)
            {
                return opponentScores.Sum();
            }

            // Exclude highest and lowest
            return opponentScores.Skip(1).Take(opponentScores.Count - 2).Sum();
        }
    }
}
