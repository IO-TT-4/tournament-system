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
            // STEP 1: Calculate main points for each participant
            var standings = participants.Select(p => new StandingsEntry
            {
                UserId = p.UserId,
                Points = matches
                    .Where(m => m.IsCompleted)
                    .Sum(m => m.PlayerHomeId == p.UserId ? (m.Result?.ScoreA ?? 0) : 
                            m.PlayerAwayId == p.UserId ? (m.Result?.ScoreB ?? 0) : 0)
            }).ToList();

            // STEP 2: Calculate tie-breaker values
            // Important: DirectEncounter needs already calculated Points in standings!
            foreach (var entry in standings)
            {
                foreach (var tb in tieBreakerPriority)
                {
                    entry.TieBreakerValues[tb.Name] = tb.Calculate(entry.UserId, matches, standings);
                }
            }

            // STEP 3: Cascading sort
            var query = standings.OrderByDescending(s => s.Points);

            foreach (var tb in tieBreakerPriority)
            {
                query = query.ThenByDescending(s => s.TieBreakerValues[tb.Name]);
            }

            var finalStandings = query.ToList();

            // STEP 4: Assign positions (handling ex-aequo)
            for (int i = 0; i < finalStandings.Count; i++)
            {
                finalStandings[i].Position = i + 1;
            }

            return finalStandings;
        }
    }
}