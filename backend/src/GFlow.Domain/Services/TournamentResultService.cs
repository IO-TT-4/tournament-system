using GFlow.Domain.Entities;
using GFlow.Domain.Services.TieBreakers;
using GFlow.Domain.ValueObjects;

namespace GFlow.Domain.Services
{
    public class TournamentResultService
    {
        public List<StandingsEntry> CalculateStandings(
            List<TournamentParticipant> participants, 
            List<Match> matches, 
            List<ITieBreaker> tieBreakerPriority)
        {
            // KROK 1: Oblicz punkty główne dla każdego
            var standings = participants.Select(p => new StandingsEntry
            {
                UserId = p.UserId,
                Points = matches
                    .Where(m => m.IsCompleted)
                    .Sum(m => m.PlayerHomeId == p.UserId ? (m.Result?.ScoreA ?? 0) : 
                            m.PlayerAwayId == p.UserId ? (m.Result?.ScoreB ?? 0) : 0)
            }).ToList();

            // KROK 2: Oblicz wartości tie-breakerów
            // Ważne: DirectEncounter potrzebuje już obliczonych Points w standings!
            foreach (var entry in standings)
            {
                foreach (var tb in tieBreakerPriority)
                {
                    entry.TieBreakerValues[tb.Name] = tb.Calculate(entry.UserId, matches, standings);
                }
            }

            // KROK 3: Kaskadowe sortowanie
            var query = standings.OrderByDescending(s => s.Points);

            foreach (var tb in tieBreakerPriority)
            {
                query = query.ThenByDescending(s => s.TieBreakerValues[tb.Name]);
            }

            var finalStandings = query.ToList();

            // KROK 4: Przypisanie pozycji (obsługa ex-aequo)
            for (int i = 0; i < finalStandings.Count; i++)
            {
                finalStandings[i].Position = i + 1;
            }

            return finalStandings;
        }
    }
}