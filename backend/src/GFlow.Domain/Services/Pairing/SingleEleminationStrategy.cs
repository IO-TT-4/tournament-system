using GFlow.Domain.Entities;
using GFlow.Domain.ValueObjects;

namespace GFlow.Domain.Services.Pairings
{
    public class SingleEleminationStrategy : IPairingStrategy
    {
        public IEnumerable<Match> GenerateNextRound(string tournamentId, List<TournamentParticipant> participants, List<Match> existingMatches)
    {
        // 1. Jeśli brak meczów -> Generujemy RUNDĘ 1 (Pierwsze piętro drabinki)
        if (!existingMatches.Any())
        {
            return GenerateFirstRound(tournamentId, participants);
        }

        // 2. Znajdź ostatnią rundę i sprawdź czy się zakończyła
        var lastRoundNumber = existingMatches.Max(m => m.RoundNumber);
        var lastRoundMatches = existingMatches.Where(m => m.RoundNumber == lastRoundNumber).ToList();

        if (lastRoundMatches.Any(m => !m.IsCompleted))
        {
            return Enumerable.Empty<Match>(); // Czekamy na wyniki wszystkich meczów w rundzie
        }

        // 3. Pobierz zwycięzców poprzedniej rundy
        var winners = lastRoundMatches
            .OrderBy(m => m.PositionInRound) // Ważne dla zachowania struktury drabinki
            .Select(m => GetWinnerId(m))
            .ToList();

        // Jeśli został tylko jeden zwycięzca, turniej się skończył
        if (winners.Count <= 1) return Enumerable.Empty<Match>();

        // 4. Parujemy zwycięzców w kolejnej rundzie
        return GenerateSubsequentRound(tournamentId, winners, lastRoundNumber + 1);
    }

    private IEnumerable<Match> GenerateFirstRound(string tournamentId, List<TournamentParticipant> participants)
    {
        var players = participants.OrderByDescending(p => p.Ranking).ToList();
        int playerCount = players.Count;

        // Oblicz rozmiar drabinki (najbliższa potęga 2)
        int bracketSize = 1;
        while (bracketSize < playerCount) bracketSize *= 2;

        int numberOfByes = bracketSize - playerCount;
        int matchesInFirstRound = bracketSize / 2;

        var matches = new List<Match>();

        for (int i = 0; i < matchesInFirstRound; i++)
        {
            var match = new Match(tournamentId, Guid.Empty.ToString(), Guid.Empty.ToString(), 1, tournamentId);
            match.PositionInRound = i + 1;

            // Logika rozstawiania (Seeding)
            // W profesjonalnych drabinkach paruje się najlepszego z najsłabszym (1 vs 16, 2 vs 15 itd.)
            var home = players[i];
            var awayIndex = (bracketSize - 1) - i;

            if (awayIndex < playerCount)
            {
                // Normalny mecz
                var away = players[awayIndex];
                match.PlayerHomeId = home.UserId;
                match.PlayerAwayId = away.UserId;
            }
            else
            {
                // BYE - gracz przechodzi automatycznie
                match.PlayerHomeId = home.UserId;
                match.PlayerAwayId = Guid.Empty.ToString();
                match.SetResult(MatchResult.CreateBye());
            }

            matches.Add(match);
        }

        return matches;
    }

    private List<Match> GenerateSubsequentRound(string tournamentId, List<string> winners, int roundNumber)
    {
        var matches = new List<Match>();
        for (int i = 0; i < winners.Count; i += 2)
        {
            var match = new Match(tournamentId, winners[i], winners[i + 1], roundNumber, tournamentId);
            match.PositionInRound = (i / 2) + 1;
            matches.Add(match);
        }
        return matches;
    }

    private string GetWinnerId(Match match)
    {
        if (match.Result == null) return Guid.Empty.ToString();
        return match.Result.ScoreA > match.Result.ScoreB ? match.PlayerHomeId : match.PlayerAwayId;
    }
    }
}