using GFlow.Domain.Entities;
using GFlow.Domain.ValueObjects;

namespace GFlow.Domain.Services.Pairings
{
    public class DoubleEliminationStrategy : IPairingStrategy
    {
        private const int LOSERS_BRACKET_OFFSET = 1000;

        public IEnumerable<Match> GenerateNextRound(Tournament tournament, List<TournamentParticipant> participants, List<Match> existingMatches)
        {
            // 1. If no matches -> Generate ROUND 1 of Winners Bracket
            if (!existingMatches.Any())
            {
                return GenerateFirstRound(tournament.Id, participants);
            }

            // 2. Check if the last generated round is finished
            var lastRoundNumber = existingMatches.Max(m => m.RoundNumber);
            var lastRoundMatches = existingMatches.Where(m => m.RoundNumber == lastRoundNumber).ToList();
            
            if (lastRoundMatches.Any(m => !m.IsCompleted))
            {
                return Enumerable.Empty<Match>();
            }

            // 3. Determine bracket state and generate next round
            return GenerateNextRoundInternal(tournament.Id, participants, existingMatches, lastRoundNumber);
        }

        private IEnumerable<Match> GenerateNextRoundInternal(string tournamentId, List<TournamentParticipant> participants, List<Match> existingMatches, int lastRoundNumber)
        {
            var nextRoundNumber = lastRoundNumber + 1;
            var nextMatches = new List<Match>();

            // Separate matches by bracket type
            var wbMatches = existingMatches.Where(m => IsWinnersBracketMatch(m)).ToList();
            var lbMatches = existingMatches.Where(m => IsLosersBracketMatch(m)).ToList();

            // Get the results of the rounds that JUST finished
            var lastRoundWBMatches = wbMatches.Where(m => m.RoundNumber == lastRoundNumber).ToList();
            var lastRoundLBMatches = lbMatches.Where(m => m.RoundNumber == lastRoundNumber).ToList();

            // Winners from the latest activities
            var wbWinnersFromLast = GetWinnersFromMatches(lastRoundWBMatches);
            var lbWinnersFromLast = GetWinnersFromMatches(lastRoundLBMatches);

            // SPECIAL: Identify WB losers who haven't entered LB yet (Pending Losers)
            var allWBLosers = GetLosersFromMatches(wbMatches);
            var allLBPlayers = lbMatches.SelectMany(m => new[] { m.PlayerHomeId, m.PlayerAwayId }).ToHashSet();
            
            // Sort by ranking to maintain seeding in LB
            var rankingMap = participants.ToDictionary(p => p.UserId, p => p.Ranking);
            var pendingWBLosers = allWBLosers
                .Where(id => !allLBPlayers.Contains(id))
                .OrderBy(id => rankingMap.ContainsKey(id) ? rankingMap[id] : 0)
                .ToList();

            bool generatedAnything = false;

            // 1. ADVANCE WINNERS BRACKET
            // If the latest WB round had > 1 winner, we definitely need a new WB round.
            if (wbWinnersFromLast.Count > 1)
            {
                nextMatches.AddRange(GenerateWinnersBracketRound(tournamentId, wbWinnersFromLast, nextRoundNumber));
                generatedAnything = true;
            }

            // 2. HANDLE LOSERS BRACKET
            // Case A: Incorporate Pending WB Losers
            // This happens either in the very first LB round OR in "Major" rounds.
            if (pendingWBLosers.Any())
            {
                if (!lbMatches.Any())
                {
                    // First LB round: pair WB losers against each other
                    nextMatches.AddRange(GenerateWinnersToLosersPairs(tournamentId, pendingWBLosers, nextRoundNumber));
                    generatedAnything = true;
                }
                else if (lbWinnersFromLast.Any())
                {
                    // Major Round: pair LB winners against pending WB losers
                    nextMatches.AddRange(GenerateLBWinnersWithWBLosers(tournamentId, lbWinnersFromLast, pendingWBLosers, nextRoundNumber));
                    generatedAnything = true;
                }
            }
            
            // Case B: Minor LB Round (Advance LB winners who haven't played together yet)
            // If we didn't incorporate WB losers but have multiple LB winners, they play each other.
            if (!generatedAnything && lbWinnersFromLast.Count > 1)
            {
                nextMatches.AddRange(GenerateLosersBracketRound(tournamentId, lbWinnersFromLast, nextRoundNumber));
                generatedAnything = true;
            }

            // 3. GRAND FINALS
            if (!generatedAnything)
            {
                var finalWBWinners = wbWinnersFromLast;
                if (finalWBWinners.Count == 0 && wbMatches.Any())
                {
                    var maxWBRound = wbMatches.Max(m => m.RoundNumber);
                    finalWBWinners = GetWinnersFromMatches(wbMatches.Where(m => m.RoundNumber == maxWBRound).ToList());
                }

                var finalLBWinners = lbWinnersFromLast;
                if (finalLBWinners.Count == 0 && lbMatches.Any())
                {
                    var maxLBRound = lbMatches.Max(m => m.RoundNumber);
                    finalLBWinners = GetWinnersFromMatches(lbMatches.Where(m => m.RoundNumber == maxLBRound).ToList());
                }

                if (finalWBWinners.Count == 1 && finalLBWinners.Count == 1 && !pendingWBLosers.Any())
                {
                    var match = new Match(Guid.NewGuid().ToString(), finalWBWinners[0], finalLBWinners[0], nextRoundNumber, tournamentId);
                    match.PositionInRound = 1;
                    nextMatches.Add(match);
                    generatedAnything = true;
                }
            }

            return nextMatches;
        }

        private IEnumerable<Match> GenerateFirstRound(string tournamentId, List<TournamentParticipant> participants)
        {
            // Round 1 is identical to Single Elimination
            var players = participants
                .Where(p => !p.IsWithdrawn)
                .OrderByDescending(p => p.Ranking)
                .ToList();
            int bracketSize = GetBracketSize(players.Count);
            int matchesInFirstRound = bracketSize / 2;

            var matches = new List<Match>();
            for (int i = 0; i < matchesInFirstRound; i++)
            {
                var match = new Match(Guid.NewGuid().ToString(), Guid.Empty.ToString(), null, 1, tournamentId);
                match.PositionInRound = i + 1; // WB position
                
                // Standard Seeding 1 vs N, 2 vs N-1
                match.PlayerHomeId = players[i].UserId;
                if (bracketSize - 1 - i < players.Count)
                    match.PlayerAwayId = players[bracketSize - 1 - i].UserId;
                else
                {
                    match.PlayerAwayId = null;
                    match.SetResult(MatchResult.CreateBye());
                }

                matches.Add(match);
            }
            return matches;
        }

        private IEnumerable<Match> GenerateWinnersBracketRound(string tournamentId, List<string> winners, int roundNumber)
        {
            var matches = new List<Match>();
            for (int i = 0; i < winners.Count; i += 2)
            {
                if (i + 1 < winners.Count)
                {
                    var match = new Match(Guid.NewGuid().ToString(), winners[i], winners[i + 1], roundNumber, tournamentId);
                    match.PositionInRound = (i / 2) + 1;
                    matches.Add(match);
                }
                else
                {
                    // Odd one out - technically shouldn't happen in standard WB power-of-2, but safe to handle with BYE or wait?
                    // Usually WB is balanced. If not, giving BYE.
                     var match = new Match(Guid.NewGuid().ToString(), winners[i], null, roundNumber, tournamentId);
                     match.PositionInRound = (i / 2) + 1;
                     match.SetResult(MatchResult.CreateBye());
                     matches.Add(match);
                }
            }
            return matches;
        }

        private IEnumerable<Match> GenerateWinnersToLosersPairs(string tournamentId, List<string> wbLosers, int roundNumber)
        {
            var matches = new List<Match>();
            for (int i = 0; i < wbLosers.Count; i += 2)
            {
                if (i + 1 < wbLosers.Count)
                {
                    var match = new Match(Guid.NewGuid().ToString(), wbLosers[i], wbLosers[i + 1], roundNumber, tournamentId);
                    match.PositionInRound = LOSERS_BRACKET_OFFSET + (i / 2) + 1;
                    matches.Add(match);
                }
                else
                {
                    // Odd loser (e.g. from BYE structure) gets a BYE in LB
                     var match = new Match(Guid.NewGuid().ToString(), wbLosers[i], null, roundNumber, tournamentId);
                     match.PositionInRound = LOSERS_BRACKET_OFFSET + (i / 2) + 1;
                     match.SetResult(MatchResult.CreateBye());
                     matches.Add(match);
                }
            }
            return matches;
        }

        private IEnumerable<Match> GenerateLBWinnersWithWBLosers(string tournamentId, List<string> lbWinners, List<string> wbLosers, int roundNumber)
        {
            var matches = new List<Match>();
            
            // We should pairs them. If one list is longer, the extras need to play each other or get BYEs?
            // Standard DE: WB Losers drop into specific slots. LB Winners play them.
            // If counts mismatch, it means the bracket isn't perfectly balanced (BYEs happened).
            // Logic: Pair 1-to-1 as much as possible.
            // prioritize pairing Max Rank vs Max Rank? Or just order.
            
            int commonCount = Math.Min(lbWinners.Count, wbLosers.Count);
            int matchIndex = 0;
            
            for (int i = 0; i < commonCount; i++)
            {
                var match = new Match(Guid.NewGuid().ToString(), lbWinners[i], wbLosers[i], roundNumber, tournamentId);
                match.PositionInRound = LOSERS_BRACKET_OFFSET + (++matchIndex);
                matches.Add(match);
            }
            
            // Handle leftovers from LB Winners (if any)
            for (int i = commonCount; i < lbWinners.Count; i++)
            {
                // They have no one to play from WB. They get a BYE for this round effectively (advance).
                var match = new Match(Guid.NewGuid().ToString(), lbWinners[i], null, roundNumber, tournamentId);
                match.PositionInRound = LOSERS_BRACKET_OFFSET + (++matchIndex);
                match.SetResult(MatchResult.CreateBye());
                matches.Add(match);
            }
            
            // Handle leftovers from WB Losers (if any)
            for (int i = commonCount; i < wbLosers.Count; i++)
            {
                // Dropped but no LB opponent. BYE.
                var match = new Match(Guid.NewGuid().ToString(), wbLosers[i], null, roundNumber, tournamentId);
                match.PositionInRound = LOSERS_BRACKET_OFFSET + (++matchIndex);
                match.SetResult(MatchResult.CreateBye());
                matches.Add(match);
            }

            return matches;
        }

        private IEnumerable<Match> GenerateLosersBracketRound(string tournamentId, List<string> lbWinners, int roundNumber)
        {
            var matches = new List<Match>();
            for (int i = 0; i < lbWinners.Count; i += 2)
            {
                if (i + 1 < lbWinners.Count)
                {
                    var match = new Match(Guid.NewGuid().ToString(), lbWinners[i], lbWinners[i + 1], roundNumber, tournamentId);
                    match.PositionInRound = LOSERS_BRACKET_OFFSET + (i / 2) + 1;
                    matches.Add(match);
                }
                else
                {
                    // Odd LB winner gets Bye
                     var match = new Match(Guid.NewGuid().ToString(), lbWinners[i], null, roundNumber, tournamentId);
                     match.PositionInRound = LOSERS_BRACKET_OFFSET + (i / 2) + 1;
                     match.SetResult(MatchResult.CreateBye());
                     matches.Add(match);
                }
            }
            return matches;
        }

        private bool IsWinnersBracketMatch(Match match)
        {
            return match.PositionInRound.HasValue && match.PositionInRound.Value < LOSERS_BRACKET_OFFSET;
        }

        private bool IsLosersBracketMatch(Match match)
        {
            return match.PositionInRound.HasValue && match.PositionInRound.Value >= LOSERS_BRACKET_OFFSET;
        }

        private int GetBracketSize(int count)
        {
            int size = 1;
            while (size < count) size *= 2;
            return size;
        }

        private List<string> GetWinnersFromMatches(List<Match> matches)
        {
            return matches
                .OrderBy(m => m.PositionInRound)
                .Select(m => m.Result!.ScoreA > m.Result.ScoreB ? m.PlayerHomeId : m.PlayerAwayId)
                .ToList();
        }

        private List<string> GetLosersFromMatches(List<Match> matches)
        {
            return matches
                .Where(m => m.PlayerAwayId != null)
                .OrderBy(m => m.PositionInRound)
                .Select(m => m.Result!.ScoreA > m.Result.ScoreB ? m.PlayerAwayId! : m.PlayerHomeId)
                .ToList();
        }
    }
}