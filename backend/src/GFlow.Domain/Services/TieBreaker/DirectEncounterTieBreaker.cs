using GFlow.Domain.Entities;
using GFlow.Domain.ValueObjects;

namespace GFlow.Domain.Services.TieBreakers
{
    public class DirectEncounterTieBreaker : ITieBreaker
    {
        public string Name => "Direct Encounter";

        public double Calculate(string userId, List<Match> allMatches, List<StandingsEntry> currentStandings)
        {
            // 1. Find points of the participant for whom you are calculating the tie-breaker
            var userPoints = currentStandings.First(s => s.UserId == userId).Points;

            // 2. Find all other players with the same points (tie group)
            var tiedPlayerIds = currentStandings
                .Where(s => s.Points == userPoints)
                .Select(s => s.UserId)
                .ToList();

            // If no one else has the same points, this tie-breaker doesn't matter (return 0)
            if (tiedPlayerIds.Count < 2) return 0;

            // 3. Calculate points earned by userId ONLY in matches with people from tiedPlayerIds
            double directPoints = 0;
            var directMatches = allMatches.Where(m => m.IsCompleted &&
                tiedPlayerIds.Contains(m.PlayerHomeId) && 
                tiedPlayerIds.Contains(m.PlayerAwayId));

            foreach (var match in directMatches)
            {
                if (match.PlayerHomeId == userId)
                    directPoints += match.Result?.ScoreA ?? 0;
                else if (match.PlayerAwayId == userId)
                    directPoints += match.Result?.ScoreB ?? 0;
            }

            return directPoints;
        }
    }
}