using GFlow.Domain.Entities;
using GFlow.Domain.ValueObjects;

namespace GFlow.Domain.Services.Pairings
{
    public class DoubleRoundRobinStrategy : IPairingStrategy
    {
        public IEnumerable<Match> GenerateNextRound(Tournament tournament, List<TournamentParticipant> participants, List<Match> existingMatches)
        {
            string tournamentId = tournament.Id;
            // 1. If matches already exist, do not generate them again
            if (existingMatches.Any())
            {
                return Enumerable.Empty<Match>();
            }

            var allMatches = new List<Match>();
            var players = participants.OrderBy(p => p.Ranking).ToList();

            // 2. Odd number of players handling
            if (players.Count % 2 != 0)
            {
                players.Add(new TournamentParticipant(Guid.Empty.ToString()));
            }

            int n = players.Count;
            int roundsInSingleCycle = n - 1;
            int matchesPerRound = n / 2;

            // --- CYCLE 1: First Round ("Home" Matches) ---
            var cycle1Matches = GenerateCycle(tournamentId, players, 1, false);
            allMatches.AddRange(cycle1Matches);

            // --- CYCLE 2: Rematches ("Away" Matches) ---
            // We start from the round number following the first cycle
            var cycle2Matches = GenerateCycle(tournamentId, players, roundsInSingleCycle + 1, true);
            allMatches.AddRange(cycle2Matches);

            return allMatches;
        }

        private List<Match> GenerateCycle(string tournamentId, List<TournamentParticipant> players, int startRound, bool isReverse)
        {
            var cycleMatches = new List<Match>();
            int n = players.Count;
            int roundsCount = n - 1;
            
            // List copy for rotation, to avoid messing up the original between cycles
            var rotationList = new List<TournamentParticipant>(players);

            for (int r = 0; r < roundsCount; r++)
            {
                int currentRoundNumber = startRound + r;

                for (int i = 0; i < n / 2; i++)
                {
                    var p1 = rotationList[i];
                    var p2 = rotationList[n - 1 - i];

                    if (p1.UserId != Guid.Empty.ToString() && p2.UserId != Guid.Empty.ToString())
                    {
                        // Role swap logic:
                        // In the first cycle (isReverse = false) we use balance (r+i)%2
                        // In the second cycle (isReverse = true) we reverse what would have come out in the first
                        bool shouldSwap = (r + i) % 2 == 0;
                        if (isReverse) shouldSwap = !shouldSwap;

                        var match = shouldSwap
                            ? new Match(tournamentId, p1.UserId, p2.UserId, currentRoundNumber, tournamentId)
                            : new Match(tournamentId, p2.UserId, p1.UserId, currentRoundNumber, tournamentId);

                        cycleMatches.Add(match);
                    }
                    else
                    {
                        // BYE Handling
                        var activePlayerId = p1.UserId == Guid.Empty.ToString() ? p2.UserId : p1.UserId;
                        var byeMatch = new Match(tournamentId, activePlayerId, Guid.Empty.ToString(), currentRoundNumber, tournamentId);
                        byeMatch.SetResult(MatchResult.CreateBye());
                        cycleMatches.Add(byeMatch);
                    }
                }

                // Standard Circle Method rotation (first stays, rest rotates)
                var last = rotationList[^1];
                rotationList.RemoveAt(rotationList.Count - 1);
                rotationList.Insert(1, last);
            }

            return cycleMatches;
        }
    }
}