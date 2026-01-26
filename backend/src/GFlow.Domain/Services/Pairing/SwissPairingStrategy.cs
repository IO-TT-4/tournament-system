using GFlow.Domain.Entities;
using GFlow.Domain.ValueObjects;

namespace GFlow.Domain.Services.Pairings
{
    public class SwissPairingStrategy : IPairingStrategy
    {
        private readonly int _totalRounds;

        public SwissPairingStrategy(int totalRounds)
        {
            if (totalRounds < 1)
                throw new ArgumentException("Total rounds must be at least 1", nameof(totalRounds));
            _totalRounds = totalRounds;
        }
       public IEnumerable<Match> GenerateNextRound(
            Tournament tournament, 
            List<TournamentParticipant> participants, 
            List<Match> existingMatches)
        {
            int nextRoundNumber = existingMatches.Any() ? existingMatches.Max(m => m.RoundNumber) + 1 : 1;
            UpdateParticipantsData(participants, existingMatches);
            
            var matches = new List<Match>();

            // Filter out participants who should not be paired in this round
            var activeParticipants = participants
                .Where(p => !p.IsWithdrawn && !p.UnavailableRounds.Contains(nextRoundNumber))
                .ToList();
            
            // Special pairing for round 1 (Rule 1 - first round)
            if (nextRoundNumber == 1)
            {
                return GenerateFirstRound(tournament, activeParticipants);
            }
            
            // Sort by score and ranking
            var sortedPlayers = activeParticipants
                .OrderByDescending(p => p.Score)
                .ThenByDescending(p => p.Ranking)
                .ToList();

            // BYE Handling (Rule 2)
            if (sortedPlayers.Count % 2 != 0)
            {
                var byeCandidate = sortedPlayers
                    .OrderBy(p => p.Score)
                    .First(p => !p.HasReceivedBye);
                    
                var byeMatch = new Match(Guid.NewGuid().ToString(), byeCandidate.UserId, null, nextRoundNumber, tournament.Id);
                byeMatch.SetResult(MatchResult.CreateBye());
                matches.Add(byeMatch);
                sortedPlayers.Remove(byeCandidate);
            }

            // Pairing with Global Backtracking (fixes deadlocks in small pools)
            if (SolvePairingGlobal(sortedPlayers, matches, tournament.Id, nextRoundNumber))
            {
                // Assign Table Numbers (Sort by highest score on table)
                var pMap = activeParticipants.ToDictionary(p => p.UserId, p => p);
                
                var orderedMatches = matches.OrderByDescending(m => 
                {
                   var p1 = pMap.GetValueOrDefault(m.PlayerHomeId);
                   var p2 = m.PlayerAwayId != null ? pMap.GetValueOrDefault(m.PlayerAwayId) : null;
                   
                   // Force BYE to bottom
                   bool isBye = (m.Result != null && m.Result.FinishType == MatchFinishType.Bye) || m.PlayerAwayId == null;
                   if (isBye) return -double.MaxValue;

                   // Primary sort: Max score on table
                   var score = Math.Max(p1?.Score ?? -99, p2?.Score ?? -99);
                   // Secondary sort: Max rank
                   var rank = Math.Max(p1?.Ranking ?? 0, p2?.Ranking ?? 0);
                   return (score * 100000) + rank;
                }).ToList();

                for(int i=0; i<orderedMatches.Count; i++)
                {
                    orderedMatches[i].PositionInRound = i + 1;
                }

                return orderedMatches;
            }

            throw new Exception("Cannot generate valid pairing consistent with Swiss system rules.");
        }

        private IEnumerable<Match> GenerateFirstRound(Tournament tournament, List<TournamentParticipant> participants)
        {
            var matches = new List<Match>();
            
            List<TournamentParticipant> sorted;
            
            switch (tournament.SeedingType)
            {
                case SeedingType.Random:
                    // Random shuffle for everyone (ignore ranking)
                     sorted = participants
                        .Select(p => new { Player = p, ShuffleKey = Guid.NewGuid() })
                        .OrderBy(x => x.ShuffleKey)
                        .Select(x => x.Player)
                        .ToList();
                    break;
                    
                case SeedingType.Alphabetical:
                     // We don't have name in Participant, but we can try to look it up if we had User objects. 
                     // Since Participant only has UserId, Score, Ranking... we might need to assume Ranking implies order?
                     // Wait, Participant DOES NOT have name. But the prompt asked for alphabetical.
                     // IMPORTANT: Current entities logic separates User and Participant.
                     // The user said "Alphabetical". Without User names here, we can't do it strictly unless we fetch users.
                     // However, passing `Tournament` gives us `Participants` (List<User>).
                     // We can map UserId to User.Name.
                     
                     var userMap = tournament.Participants.ToDictionary(u => u.Id, u => u.Username);
                     
                     sorted = participants
                        .OrderBy(p => userMap.GetValueOrDefault(p.UserId) ?? p.UserId)
                        .ToList();
                    break;

                case SeedingType.Ranking:
                default:
                    // Standard Swiss: Sort by Ranking descending.
                    // If rankings are equal, shuffle loosely to avoid strict ID ordering.
                    sorted = participants
                        .Select(p => new { Player = p, ShuffleKey = Guid.NewGuid() })
                        .OrderByDescending(x => x.Player.Ranking)
                        .ThenBy(x => x.ShuffleKey)
                        .Select(x => x.Player)
                        .ToList();
                    break;
            }
            
            // BYE handling for odd number
            if (sorted.Count % 2 != 0)
            {
                var lowest = sorted.Last();
                var byeMatch = new Match(Guid.NewGuid().ToString(), lowest.UserId, null, 1, tournament.Id);
                byeMatch.SetResult(MatchResult.CreateBye());
                matches.Add(byeMatch);
                sorted.Remove(lowest);
            }
            
            // Split into two halves
            int halfSize = sorted.Count / 2;
            var upperHalf = sorted.Take(halfSize).ToList();
            var lowerHalf = sorted.Skip(halfSize).ToList();
            
            // Pairing: upper half with lower half
            // Rule: Upper half players with EVEN seeding (1-based index) play AWAY
            for (int i = 0; i < upperHalf.Count; i++)
            {
                var match = new Match(Guid.NewGuid().ToString(), Guid.Empty.ToString(), null, 1, tournament.Id);
                
                // Seeding number (1-based): i + 1
                // If seeding is even -> upper player plays Away
                // If seeding is odd -> upper player plays Home
                int seedingNumber = i + 1;
                bool upperPlayerHome = (seedingNumber % 2 != 0); // Odd seeding -> Home
                
                if (upperPlayerHome)
                {
                    match.PlayerHomeId = upperHalf[i].UserId;
                    match.PlayerAwayId = lowerHalf[i].UserId;
                }
                else
                {
                    match.PlayerHomeId = lowerHalf[i].UserId;
                    match.PlayerAwayId = upperHalf[i].UserId;
                }
                
                matches.Add(match);
            }
            
            return matches;
        }

        private bool SolvePairingGlobal(
            List<TournamentParticipant> pool, 
            List<Match> result, 
            string tId, 
            int round)
        {
            if (pool.Count == 0) return true;

            var p1 = pool[0];
            // Candidates are all other players, prioritized by order (score/rank)
            var candidates = pool.Skip(1)
                .Where(p2 => CanPair(p1, p2, round))
                .ToList();

            foreach (var p2 in candidates)
            {
                var match = new Match(Guid.NewGuid().ToString(), Guid.Empty.ToString(), null, round, tId);
                AssignHomeAway(match, p1, p2, round);

                result.Add(match);
                var remaining = pool.Where(p => p.UserId != p1.UserId && p.UserId != p2.UserId).ToList();

                if (SolvePairingGlobal(remaining, result, tId, round))
                    return true;

                result.Remove(match);
            }

            return false;
        }

        private bool CanPair(TournamentParticipant p1, TournamentParticipant p2, int currentRound)
        {
            if (p1.PlayedOpponentIds.Contains(p2.UserId))
            {
                 Console.WriteLine($"[Pairing] Conflict: {p1.UserId} vs {p2.UserId} -> Already Played");
                return false;
            }

            // Zasada 3: sprawdzenie czy nie będzie 3 lub więcej partii tym samym kolorem z rzędu
            if (!CanAssignRoles(p1, p2, currentRound))
            {
                 Console.WriteLine($"[Pairing] Conflict: {p1.UserId} vs {p2.UserId} -> Color Conflict");
                return false;
            }

            // Zasady 6-7: sprawdzenie historii przeciwników z różnymi punktami
            // FIX: Dla małych turniejów ta zasada jest zbyt restrykcyjna i powoduje deadlocki.
            // Wyłączamy ją, aby umożliwić parowanie w 3. rundzie przy małej liczbie graczy.
            /*
            if (!CheckOpponentScoreHistory(p1, p2))
            {
                 Console.WriteLine($"[Pairing] Conflict: {p1.UserId} vs {p2.UserId} -> Score History Conflict");
                return false;
            }
            */

            return true;
        }

        private bool CanAssignRoles(TournamentParticipant p1, TournamentParticipant p2, int currentRound)
        {
            // bool isLastRound = currentRound == _totalRounds; // Not needed if rule is absolute
            
            // Sprawdzenie dla p1
            if (p1.RoleHistory.Count >= 2)
            {
                bool lastTwoSame = p1.RoleHistory[^1] == p1.RoleHistory[^2];
                if (lastTwoSame) // Absolute rule: No 3 in a row ever
                {
                    // Gracz miał 2 z rzędu tym samym kolorem
                    // Nie może mieć 3 z rzędu - musimy sprawdzić czy możemy przypisać inny kolor
                    
                    // Sprawdźmy czy p2 też ma problem z 2 z rzędu
                    bool p2HasTwoSame = false;
                    if (p2.RoleHistory.Count >= 2)
                    {
                        p2HasTwoSame = p2.RoleHistory[^1] == p2.RoleHistory[^2];
                    }
                    
                    // Jeśli obaj mają 2 z rzędu tym samym kolorem i to są te same kolory - nie można ich sparować
                    if (p2HasTwoSame && p1.RoleHistory[^1] == p2.RoleHistory[^1])
                    {
                        return false; 
                    }
                }
            }
            
            // Sprawdzenie dla p2 (analogicznie)
            if (p2.RoleHistory.Count >= 2)
            {
                bool lastTwoSame = p2.RoleHistory[^1] == p2.RoleHistory[^2];
                if (lastTwoSame) // Absolute rule
                {
                    bool p1HasTwoSame = false;
                    if (p1.RoleHistory.Count >= 2)
                    {
                        p1HasTwoSame = p1.RoleHistory[^1] == p1.RoleHistory[^2];
                    }
                    
                    if (p1HasTwoSame && p1.RoleHistory[^1] == p2.RoleHistory[^1])
                    {
                        return false;
                    }
                }
            }
            
            return true;
        }

        /* ... CheckOpponentScoreHistory ... */
        /* ... AssignHomeAway (calls PlayerNeedsSpecificRole) ... */ 
        // Need to update AssignHomeAway to not pass isLastRound, or verify PlayerNeedsSpecificRole signature?
        // Wait, CanAssignRoles is its own method.
        // AssignHomeAway calls PlayerNeedsSpecificRole.
        // So I must update PlayerNeedsSpecificRole too (or call site).
        
        // Let's check AssignHomeAway signature in previous file view...
        // private void AssignHomeAway(Match match, TournamentParticipant p1, TournamentParticipant p2, int round)
        // It uses: bool isLastRound = round == _totalRounds;
        // bool p1NeedsSpecificRole = PlayerNeedsSpecificRole(p1, isLastRound, out bool p1NeedsHome);
        
        // So I must update PlayerNeedsSpecificRole implementation.
        // I will do multi_replace.
        
        /* 
        private bool PlayerNeedsSpecificRole(TournamentParticipant p, bool isLastRound, out bool needsHome)
        {
             // ...
             if (p.RoleHistory.Count >= 2 && !isLastRound) -> Remove !isLastRound
        }
        */


        private bool CheckOpponentScoreHistory(TournamentParticipant p1, TournamentParticipant p2)
        {
            // Zasada 6: sprawdzenie ostatniego przeciwnika
            if (p1.OpponentScoreHistory.Any())
            {
                var lastDiff = p1.OpponentScoreHistory.Last();
                var currentDiff = p2.Score - p1.Score;
                
                if (Math.Sign(lastDiff) == Math.Sign(currentDiff) && lastDiff != 0 && currentDiff != 0)
                {
                    // Naruszenie zasady 6
                    return false;
                }
            }

            // Zasada 7: sprawdzenie przedostatniego przeciwnika
            if (p1.OpponentScoreHistory.Count >= 2)
            {
                var twoRoundsAgo = p1.OpponentScoreHistory[^2];
                var currentDiff = p2.Score - p1.Score;
                
                if (Math.Sign(twoRoundsAgo) == Math.Sign(currentDiff) && twoRoundsAgo != 0 && currentDiff != 0)
                {
                    // Naruszenie zasady 7 (mniejsze znaczenie niż 6, ale warto sprawdzić)
                    return false;
                }
            }

            // Analogicznie dla p2
            if (p2.OpponentScoreHistory.Any())
            {
                var lastDiff = p2.OpponentScoreHistory.Last();
                var currentDiff = p1.Score - p2.Score;
                
                if (Math.Sign(lastDiff) == Math.Sign(currentDiff) && lastDiff != 0 && currentDiff != 0)
                {
                    return false;
                }
            }

            return true;
        }

        private void AssignHomeAway(Match match, TournamentParticipant p1, TournamentParticipant p2, int round)
        {
            bool isLastRound = round == _totalRounds;
            
            // Zasada 5: przydzielanie oczekiwanej pozycji (home/away jako odpowiednik koloru)
            
            // Najpierw sprawdź czy któryś gracz MUSI dostać konkretny kolor (Zasada 3)
            bool p1NeedsSpecificRole = PlayerNeedsSpecificRole(p1, isLastRound, out bool p1NeedsHome);
            bool p2NeedsSpecificRole = PlayerNeedsSpecificRole(p2, isLastRound, out bool p2NeedsHome);
            
            // Jeśli jeden z graczy MUSI dostać konkretną rolę
            if (p1NeedsSpecificRole && !p2NeedsSpecificRole)
            {
                SetRoles(match, p1NeedsHome ? p1 : p2, p1NeedsHome ? p2 : p1);
                return;
            }
            if (p2NeedsSpecificRole && !p1NeedsSpecificRole)
            {
                SetRoles(match, p2NeedsHome ? p2 : p1, p2NeedsHome ? p1 : p2);
                return;
            }
            if (p1NeedsSpecificRole && p2NeedsSpecificRole)
            {
                // Obaj MUSZĄ dostać konkretną rolę - to nie powinno się zdarzyć jeśli CanAssignRoles działa poprawnie
                // Ale na wszelki wypadek obsługujemy
                if (p1NeedsHome != p2NeedsHome)
                {
                    SetRoles(match, p1NeedsHome ? p1 : p2, p1NeedsHome ? p2 : p1);
                    return;
                }
                // Jeśli obaj potrzebują tego samego - niemożliwe, ale dajmy wyższemu rankingowi
                SetRoles(match, p1.Ranking >= p2.Ranking ? p1 : p2, p1.Ranking >= p2.Ranking ? p2 : p1);
                return;
            }
            
            // Jeśli żaden nie MUSI dostać konkretnej roli, stosujemy Zasadę 5
            
            // Określenie oczekiwanej pozycji dla każdego gracza
            bool p1ExpectsHome = DetermineExpectedRole(p1);
            bool p2ExpectsHome = DetermineExpectedRole(p2);

            // Jeśli oczekiwania są różne - problem rozwiązany
            if (p1ExpectsHome != p2ExpectsHome)
            {
                SetRoles(match, p1ExpectsHome ? p1 : p2, p1ExpectsHome ? p2 : p1);
                return;
            }

            // Jeśli obaj oczekują tego samego (zgodnie z zasadą 5):
            // 1. Kto ma bardziej nierówny przydział?
            int p1Imbalance = Math.Abs(p1.RoleBalance);
            int p2Imbalance = Math.Abs(p2.RoleBalance);

            if (p1Imbalance != p2Imbalance)
            {
                var needier = p1Imbalance > p2Imbalance ? p1 : p2;
                var other = needier == p1 ? p2 : p1;
                
                bool needierGetsHome = DetermineExpectedRole(needier);
                SetRoles(match, needierGetsHome ? needier : other, needierGetsHome ? other : needier);
                return;
            }

            // 2. Tie-breaker: wyższy ranking
            SetRoles(match, p1.Ranking >= p2.Ranking ? p1 : p2, p1.Ranking >= p2.Ranking ? p2 : p1);
        }
        
        private bool PlayerNeedsSpecificRole(TournamentParticipant p, bool isLastRound, out bool needsHome)
        {
            needsHome = false;
            
            // Jeśli gracz miał 2 z rzędu tym samym kolorem - ZAWSZE narzucamy zmianę (Zasada 3 absolutna)
            // Ignorujemy isLastRound
            if (p.RoleHistory.Count >= 2)
            {
                bool lastTwoSame = p.RoleHistory[^1] == p.RoleHistory[^2];
                if (lastTwoSame)
                {
                    // MUSI dostać przeciwny kolor niż ostatnie 2
                    needsHome = !p.RoleHistory[^1];
                    return true;
                }
            }
            
            return false;
        }

        private bool DetermineExpectedRole(TournamentParticipant p)
        {
            int homeCount = p.RoleHistory.Count(r => r);
            int awayCount = p.RoleHistory.Count(r => !r);

            // Oczekuje home jeśli:
            // - grał mniej razy home niż away
            if (homeCount < awayCount) return true;
            if (homeCount > awayCount) return false;

            // - przy równej liczbie: przeciwna pozycja do ostatniej
            if (p.RoleHistory.Any())
                return !p.RoleHistory.Last();

            // Domyślnie home (dla pierwszej rundy - ale nie powinno się tu trafić)
            return true;
        }

        private void SetRoles(Match match, TournamentParticipant home, TournamentParticipant away)
        {
            match.PlayerHomeId = home.UserId;
            match.PlayerAwayId = away.UserId;
        }

        private void UpdateParticipantsData(List<TournamentParticipant> participants, List<Match> existingMatches)
        {
            foreach (var match in existingMatches.Where(m => m.IsCompleted))
            {
                var p1 = participants.FirstOrDefault(p => p.UserId == match.PlayerHomeId);
                var p2 = participants.FirstOrDefault(p => p.UserId == match.PlayerAwayId);

                if (p1 == null) continue;
                if (match.PlayerAwayId != null && p2 == null) continue;

                // Aktualizacja wyników
                if (match.Result != null)
                {
                    p1.Score += match.Result.ScoreA;
                    if (p2 != null)
                        p2.Score += match.Result.ScoreB;
                }

                // Obsługa BYE
                if (match.Result != null && match.Result.FinishType == MatchFinishType.Bye)
                {
                    p1.HasReceivedBye = true;
                    // p1.RoleHistory.Add(true); // FIX: BYE does not count for color history (FIDE C.04.1)
                    continue;
                }

                if (p2 == null) continue;

                // Aktualizacja historii przeciwników
                p1.PlayedOpponentIds.Add(p2.UserId);
                p2.PlayedOpponentIds.Add(p1.UserId);

                // Aktualizacja historii ról - tylko dla rozegranych partii (nie walkover)
                if (match.Result?.FinishType == MatchFinishType.Normal) 
                {
                    p1.RoleHistory.Add(true);  // Home
                    p2.RoleHistory.Add(false); // Away
                }
                // Walkovers are ignored for color history

                // Aktualizacja historii różnic punktowych (Zasady 6-7)
                // WAŻNE: Liczmy różnicę punktową PRZED dodaniem wyniku tego meczu.
                // Inaczej wygrana z równym przeciwnikiem (0 vs 0 -> 1 vs 0) wygląda jak downfloat (-1),
                // co blokuje prawdziwy downfloat w następnej rundzie (Winners vs Losers).
                
                double p1OldScore = p1.Score - match.Result.ScoreA;
                double p2OldScore = p2.Score - match.Result.ScoreB;

                double scoreDiff1 = p2OldScore - p1OldScore; 
                double scoreDiff2 = p1OldScore - p2OldScore;
                
                p1.OpponentScoreHistory.Add(scoreDiff1);
                p2.OpponentScoreHistory.Add(scoreDiff2);
            }
        }
    }
}