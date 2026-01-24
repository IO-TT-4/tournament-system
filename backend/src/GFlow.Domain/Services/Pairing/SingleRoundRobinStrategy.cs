using GFlow.Domain.Entities;
using GFlow.Domain.ValueObjects;

namespace GFlow.Domain.Services.Pairings
{
    public class SingleRoundRobinStrategy : IPairingStrategy
    {
        public IEnumerable<Match> GenerateNextRound(Tournament tournament, List<TournamentParticipant> participants, List<Match> existingMatches)
    {
        string tournamentId = tournament.Id;
        // 1. Check if any round has already been generated
        if (existingMatches.Any())
        {
            // In Round Robin we usually generate everything at the start.
            // If DB is not empty, it means rounds already exist.
            return Enumerable.Empty<Match>();
        }

        var allMatches = new List<Match>();
        var players = participants
            .Where(p => !p.IsWithdrawn)
            .OrderBy(p => p.Ranking)
            .ToList();

        // 2. Odd number of players handling (Ghost/BYE)
        if (players.Count % 2 != 0)
        {
            // Add "empty" player to even out the pair
            players.Add(new TournamentParticipant(Guid.Empty.ToString()));
        }

        int playerCount = players.Count;
        int totalRounds = playerCount - 1;
        int matchesPerRound = playerCount / 2;

        // 3. Rotation Algorithm (Circle Method)
        for (int round = 1; round <= totalRounds; round++)
        {
            for (int i = 0; i < matchesPerRound; i++)
            {
                var home = players[i];
                var away = players[playerCount - 1 - i];

                // Skip match if one of the players is "Ghost" (BYE)
                if (home.UserId != Guid.Empty.ToString() && away.UserId != Guid.Empty.ToString())
                {
                    // Alternating Home/Away role assignment for balance
                    var match = (round + i) % 2 == 0 
                        ? new Match(tournamentId, home.UserId, away.UserId, round, tournamentId)
                        : new Match(tournamentId, away.UserId, home.UserId, round, tournamentId);
                    
                    allMatches.Add(match);
                }
                else
                {
                    // Optional: You can save the BYE match in the database with a ready result
                    var byePlayerId = home.UserId == Guid.Empty.ToString() ? away.UserId : home.UserId;
                    var byeMatch = new Match(tournamentId, byePlayerId, Guid.Empty.ToString(), round, tournamentId);
                    byeMatch.SetResult(MatchResult.CreateBye());
                    allMatches.Add(byeMatch);
                }
            }

            // Rotation: Fixed first element, rest shifts right
            // Example for 4 players: 
            // R1: (1,4) (2,3) -> R2: (1,3) (4,2) -> R3: (1,2) (3,4)
            var lastPlayer = players[^1];
            players.RemoveAt(players.Count - 1);
            players.Insert(1, lastPlayer);
        }

        return allMatches;
    }
    }
}