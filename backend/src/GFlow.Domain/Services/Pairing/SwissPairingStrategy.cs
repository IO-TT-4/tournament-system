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
            string tournamentId, 
            List<TournamentParticipant> participants, 
            List<Match> existingMatches)
        {
            int nextRoundNumber = existingMatches.Any() ? existingMatches.Max(m => m.RoundNumber) + 1 : 1;
            UpdateParticipantsData(participants, existingMatches);
            
            var matches = new List<Match>();
            
            // Specjalne kojarzenie dla rundy 1 (Zasada 1 - pierwsza runda)
            if (nextRoundNumber == 1)
            {
                return GenerateFirstRound(tournamentId, participants);
            }
            
            // Sortowanie według punktów i rankingu
            var sortedPlayers = participants
                .OrderByDescending(p => p.Score)
                .ThenByDescending(p => p.Ranking)
                .ToList();

            // Obsługa BYE (Zasada 2)
            if (sortedPlayers.Count % 2 != 0)
            {
                var byeCandidate = sortedPlayers
                    .OrderBy(p => p.Score)
                    .First(p => !p.HasReceivedBye);
                    
                var byeMatch = new Match(tournamentId, byeCandidate.UserId, Guid.Empty.ToString(), nextRoundNumber, tournamentId);
                byeMatch.SetResult(MatchResult.CreateBye());
                matches.Add(byeMatch);
                sortedPlayers.Remove(byeCandidate);
            }

            // Kojarzenie w obrębie grup punktowych (Zasada 4)
            if (SolvePairingWithScoreGroups(sortedPlayers, matches, tournamentId, nextRoundNumber))
            {
                return matches;
            }

            throw new Exception("Nie można wygenerować poprawnego kojarzenia zgodnego z zasadami systemu szwajcarskiego.");
        }

        private IEnumerable<Match> GenerateFirstRound(string tournamentId, List<TournamentParticipant> participants)
        {
            var matches = new List<Match>();
            var sorted = participants.OrderByDescending(p => p.Ranking).ToList();
            
            // Obsługa BYE dla nieparzystej liczby
            if (sorted.Count % 2 != 0)
            {
                var lowest = sorted.Last();
                var byeMatch = new Match(tournamentId, lowest.UserId, Guid.Empty.ToString(), 1, tournamentId);
                byeMatch.SetResult(MatchResult.CreateBye());
                matches.Add(byeMatch);
                sorted.Remove(lowest);
            }
            
            // Podział na dwie połowy
            int halfSize = sorted.Count / 2;
            var upperHalf = sorted.Take(halfSize).ToList();
            var lowerHalf = sorted.Skip(halfSize).ToList();
            
            // Losowanie pierwszej pozycji dla pierwszej pary
            var random = new Random();
            bool firstPlayerHome = random.Next(2) == 0;
            
            // Kojarzenie: górna połowa z dolną
            for (int i = 0; i < upperHalf.Count; i++)
            {
                var match = new Match(tournamentId, Guid.Empty.ToString(), Guid.Empty.ToString(), 1, tournamentId);
                
                // Alternacja pozycji po pierwszej parze
                bool upperPlayerHome = (i == 0) ? firstPlayerHome : !matches.Last().IsPlayerHome(upperHalf[i - 1].UserId);
                
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

        private bool SolvePairingWithScoreGroups(
            List<TournamentParticipant> players, 
            List<Match> result, 
            string tId, 
            int round)
        {
            // Grupowanie według punktów (Zasada 4)
            var scoreGroups = players
                .GroupBy(p => p.Score)
                .OrderByDescending(g => g.Key)
                .Select(g => g.ToList())
                .ToList();

            var unpaired = new List<TournamentParticipant>();
            
            foreach (var group in scoreGroups)
            {
                var currentPool = unpaired.Concat(group).ToList();
                unpaired.Clear();
                
                // Próba parowania w obrębie grupy + floaters
                if (!SolvePairingInPool(currentPool, result, unpaired, tId, round))
                {
                    return false;
                }
            }
            
            // Sprawdzenie czy wszyscy zostali sparowani
            return unpaired.Count == 0;
        }

        private bool SolvePairingInPool(
            List<TournamentParticipant> pool,
            List<Match> result,
            List<TournamentParticipant> floaters,
            string tId,
            int round)
        {
            if (pool.Count == 0) return true;
            if (pool.Count == 1)
            {
                floaters.Add(pool[0]);
                return true;
            }

            var p1 = pool[0];
            var candidates = pool.Skip(1)
                .Where(p2 => CanPair(p1, p2, round))
                .ToList();

            foreach (var p2 in candidates)
            {
                var match = new Match(tId, Guid.Empty.ToString(), Guid.Empty.ToString(), round, tId);
                AssignHomeAway(match, p1, p2, round);

                result.Add(match);
                var nextPool = pool.Where(p => p.UserId != p1.UserId && p.UserId != p2.UserId).ToList();

                if (SolvePairingInPool(nextPool, result, floaters, tId, round))
                    return true;

                result.Remove(match);
            }

            // Jeśli nie udało się sparować, dodaj do floaters
            floaters.Add(p1);
            return SolvePairingInPool(pool.Skip(1).ToList(), result, floaters, tId, round);
        }

        private bool CanPair(TournamentParticipant p1, TournamentParticipant p2, int currentRound)
        {
            // Zasada 1 (podstawowa): nie grali ze sobą wcześniej
            if (p1.PlayedOpponentIds.Contains(p2.UserId))
                return false;

            // Zasada 3: sprawdzenie czy nie będzie 3 lub więcej partii tym samym kolorem z rzędu
            // (chyba że to ostatnia runda - wtedy dopuszczamy 3)
            if (!CanAssignRoles(p1, p2, currentRound))
                return false;

            // Zasady 6-7: sprawdzenie historii przeciwników z różnymi punktami
            if (!CheckOpponentScoreHistory(p1, p2))
                return false;

            return true;
        }

        private bool CanAssignRoles(TournamentParticipant p1, TournamentParticipant p2, int currentRound)
        {
            bool isLastRound = currentRound == _totalRounds;
            
            // Sprawdzenie dla p1
            if (p1.RoleHistory.Count >= 2)
            {
                bool lastTwoSame = p1.RoleHistory[^1] == p1.RoleHistory[^2];
                if (lastTwoSame && !isLastRound)
                {
                    // Gracz miał 2 z rzędu tym samym kolorem i to NIE jest ostatnia runda
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
                        return false; // Niemożliwe do sparowania bez naruszenia zasady 3
                    }
                }
            }
            
            // Sprawdzenie dla p2 (analogicznie)
            if (p2.RoleHistory.Count >= 2)
            {
                bool lastTwoSame = p2.RoleHistory[^1] == p2.RoleHistory[^2];
                if (lastTwoSame && !isLastRound)
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
            
            // Jeśli gracz miał 2 z rzędu tym samym kolorem i to NIE jest ostatnia runda
            if (p.RoleHistory.Count >= 2 && !isLastRound)
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

                if (p1 == null || (p2 == null && match.PlayerAwayId != Guid.Empty.ToString()))
                    continue;

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
                    p1.RoleHistory.Add(true);
                    continue;
                }

                if (p2 == null) continue;

                // Aktualizacja historii przeciwników
                p1.PlayedOpponentIds.Add(p2.UserId);
                p2.PlayedOpponentIds.Add(p1.UserId);

                // Aktualizacja historii ról
                p1.RoleHistory.Add(true);  // Home
                p2.RoleHistory.Add(false); // Away

                // Aktualizacja historii różnic punktowych (Zasady 6-7)
                double scoreDiff1 = p2.Score - p1.Score; // różnica przed tym meczem
                double scoreDiff2 = p1.Score - p2.Score;
                p1.OpponentScoreHistory.Add(scoreDiff1);
                p2.OpponentScoreHistory.Add(scoreDiff2);
            }
        }
    }
}