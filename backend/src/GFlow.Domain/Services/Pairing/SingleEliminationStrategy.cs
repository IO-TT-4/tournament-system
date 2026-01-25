using GFlow.Domain.Entities;
using GFlow.Domain.ValueObjects;

namespace GFlow.Domain.Services.Pairings
{
    public class SingleEliminationStrategy : IPairingStrategy
    {
        public IEnumerable<Match> GenerateNextRound(Tournament tournament, List<TournamentParticipant> participants, List<Match> existingMatches)
    {
        string tournamentId = tournament.Id;
        // 1. If no matches -> Generate ROUND 1 (First bracket level)
        if (!existingMatches.Any())
        {
            return GenerateFirstRound(tournamentId, participants);
        }

        // 2. Find last round and check if it is finished
        var lastRoundNumber = existingMatches.Max(m => m.RoundNumber);
        var lastRoundMatches = existingMatches.Where(m => m.RoundNumber == lastRoundNumber).ToList();

        if (lastRoundMatches.Any(m => !m.IsCompleted))
        {
            return Enumerable.Empty<Match>(); // Waiting for all match results in the round
        }

        // 3. Get winners from the previous round
        var winners = lastRoundMatches
            .OrderBy(m => m.PositionInRound) // Important for preserving bracket structure
            .Select(m => GetWinnerId(m))
            .ToList();

        // If only one winner remaining, tournament is over
        if (winners.Count <= 1) return Enumerable.Empty<Match>();

        // 4. Pair winners in the next round
        return GenerateSubsequentRound(tournamentId, winners, lastRoundNumber + 1);
    }

    private IEnumerable<Match> GenerateFirstRound(string tournamentId, List<TournamentParticipant> participants)
    {
        var players = participants
            .Where(p => !p.IsWithdrawn)
            .OrderByDescending(p => p.Ranking)
            .ToList();
        int playerCount = players.Count;

        // Calculate bracket size (nearest power of 2)
        int bracketSize = 1;
        while (bracketSize < playerCount) bracketSize *= 2;

        int numberOfByes = bracketSize - playerCount;
        int matchesInFirstRound = bracketSize / 2;

        var matches = new List<Match>();

        for (int i = 0; i < matchesInFirstRound; i++)
        {
            var match = new Match(tournamentId, Guid.Empty.ToString(), Guid.Empty.ToString(), 1, tournamentId);
            match.PositionInRound = i + 1;

            // Seeding logic
            // In professional brackets, pair best with worst (1 vs 16, 2 vs 15 etc.)
            var home = players[i];
            var awayIndex = (bracketSize - 1) - i;

            if (awayIndex < playerCount)
            {
                // Normal match
                var away = players[awayIndex];
                match.PlayerHomeId = home.UserId;
                match.PlayerAwayId = away.UserId;
            }
            else
            {
                // BYE - player advances automatically
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