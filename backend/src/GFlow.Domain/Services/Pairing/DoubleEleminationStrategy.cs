using GFlow.Domain.Entities;
using GFlow.Domain.ValueObjects;

namespace GFlow.Domain.Services.Pairings
{
    public class DoubleEleminationStrategy : IPairingStrategy
    {
        public IEnumerable<Match> GenerateNextRound(string tournamentId, List<TournamentParticipant> participants, List<Match> existingMatches)
        {
            // 1. Jeśli brak meczów -> Generujemy RUNDĘ 1 Drabinki Zwycięzców
            if (!existingMatches.Any())
            {
                return GenerateFirstRound(tournamentId, participants);
            }

            // Sprawdzamy czy ostatnia wygenerowana runda jest zakończona
            var lastRoundNumber = existingMatches.Max(m => m.RoundNumber);
            if (existingMatches.Where(m => m.RoundNumber == lastRoundNumber).Any(m => !m.IsCompleted))
            {
                return Enumerable.Empty<Match>();
            }

            var nextRoundMatches = new List<Match>();
            int nextRoundNumber = lastRoundNumber + 1;

            // Logika Double Elimination wymaga analizy obu drabinek równolegle.
            // W uproszczeniu: co każdą rundę Winners Bracket, w Losers Bracket odbywają się DWIE rundy:
            // - Runda "Minor": grają ci, co wygrali w Losers Bracket.
            // - Runda "Major": zwycięzcy Minor grają z tymi, którzy właśnie spadli z Winners Bracket.

            // Pobieramy wyniki wszystkich meczów, by zidentyfikować "Zwycięzców" i "Przegranych"
            var winners = GetWinnersFromRound(existingMatches, lastRoundNumber);
            var losers = GetLosersFromRound(existingMatches, lastRoundNumber);

            // ... Tutaj następuje zaawansowana logika parowania (poniżej szkielet generowania) ...
            
            return nextRoundMatches;
        }

        private IEnumerable<Match> GenerateFirstRound(string tournamentId, List<TournamentParticipant> participants)
        {
            // Runda 1 jest identyczna jak w Single Elimination
            var players = participants.OrderByDescending(p => p.Ranking).ToList();
            int bracketSize = GetBracketSize(players.Count);
            int matchesInFirstRound = bracketSize / 2;

            var matches = new List<Match>();
            for (int i = 0; i < matchesInFirstRound; i++)
            {
                var match = new Match(tournamentId, Guid.Empty.ToString(), Guid.Empty.ToString(), 1, tournamentId);
                match.PositionInRound = i + 1;
                
                // Standardowy Seeding 1 vs N, 2 vs N-1
                match.PlayerHomeId = players[i].UserId;
                if (bracketSize - 1 - i < players.Count)
                    match.PlayerAwayId = players[bracketSize - 1 - i].UserId;
                else
                    match.SetResult(MatchResult.CreateBye());

                matches.Add(match);
            }
            return matches;
        }

        private int GetBracketSize(int count)
        {
            int size = 1;
            while (size < count) size *= 2;
            return size;
        }

        private List<string> GetWinnersFromRound(List<Match> matches, int round)
        {
            return matches.Where(m => m.RoundNumber == round)
                        .Select(m => m.Result!.ScoreA > m.Result.ScoreB ? m.PlayerHomeId : m.PlayerAwayId)
                        .ToList();
        }

        private List<string> GetLosersFromRound(List<Match> matches, int round)
        {
            return matches.Where(m => m.RoundNumber == round && m.PlayerAwayId != Guid.Empty.ToString())
                        .Select(m => m.Result!.ScoreA > m.Result.ScoreB ? m.PlayerAwayId : m.PlayerHomeId)
                        .ToList();
        }
    }
}