using GFlow.Domain.Entities;
using GFlow.Domain.ValueObjects;

namespace GFlow.Domain.Services.TieBreakers
{
    public class DirectEncounterTieBreaker : ITieBreaker
    {
        public string Name => "Direct Encounter";

        public double Calculate(string userId, List<Match> allMatches, List<StandingsEntry> currentStandings)
        {
            // 1. Znajdź punkty zawodnika, dla którego liczysz tie-breaker
            var userPoints = currentStandings.First(s => s.UserId == userId).Points;

            // 2. Znajdź wszystkich innych graczy, którzy mają tyle samo punktów (grupa remisowa)
            var tiedPlayerIds = currentStandings
                .Where(s => s.Points == userPoints)
                .Select(s => s.UserId)
                .ToList();

            // Jeśli nikt inny nie ma tylu samo punktów, ten tie-breaker nie ma znaczenia (zwracamy 0)
            if (tiedPlayerIds.Count < 2) return 0;

            // 3. Oblicz punkty zdobyte przez userId TYLKO w meczach z osobami z tiedPlayerIds
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