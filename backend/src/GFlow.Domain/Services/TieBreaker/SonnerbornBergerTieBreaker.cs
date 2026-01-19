using GFlow.Domain.Entities;
using GFlow.Domain.ValueObjects;

namespace GFlow.Domain.Services.TieBreakers
{
    public class SonnerbornBergerTieBreaker : ITieBreaker
    {
        public string Name => "Sonneborn-Berger";
        public double Calculate(string userId, List<Match> allMatches, List<StandingsEntry> currentStandings)
        {
            double score = 0;
            var playedMatches = allMatches.Where(m => m.IsCompleted && (m.PlayerHomeId == userId || m.PlayerAwayId == userId));

            foreach (var match in playedMatches)
            {
                var opponentId = match.PlayerHomeId == userId ? match.PlayerAwayId : match.PlayerHomeId;
                var opponentEntry = currentStandings.FirstOrDefault(s => s.UserId == opponentId);
                if (opponentEntry == null) continue;

                var result = match.Result!;
                // Jeśli wygraliśmy z tym przeciwnikiem
                if ((match.PlayerHomeId == userId && result.ScoreA > result.ScoreB) ||
                    (match.PlayerAwayId == userId && result.ScoreB > result.ScoreA))
                {
                    score += opponentEntry.Points;
                }
                // Jeśli zremisowaliśmy
                else if (result.ScoreA == result.ScoreB)
                {
                    score += (opponentEntry.Points * 0.5);
                }
            }
            return score;
        }
    }
}