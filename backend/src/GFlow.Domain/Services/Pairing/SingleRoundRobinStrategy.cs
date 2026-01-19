using GFlow.Domain.Entities;
using GFlow.Domain.ValueObjects;

namespace GFlow.Domain.Services.Pairings
{
    class SingleRoundRobinStrategy : IPairingStrategy
    {
        public IEnumerable<Match> GenerateNextRound(string tournamentId, List<TournamentParticipant> participants, List<Match> existingMatches)
    {
        // 1. Sprawdź, czy jakakolwiek runda została już wygenerowana
        if (existingMatches.Any())
        {
            // W Round Robin zazwyczaj generujemy wszystko na starcie.
            // Jeśli baza nie jest pusta, oznacza to, że rundy już istnieją.
            return Enumerable.Empty<Match>();
        }

        var allMatches = new List<Match>();
        var players = participants.OrderBy(p => p.Ranking).ToList();

        // 2. Obsługa nieparzystej liczby graczy (Duch/BYE)
        if (players.Count % 2 != 0)
        {
            // Dodajemy "pustego" gracza, aby wyrównać do pary
            players.Add(new TournamentParticipant(Guid.Empty.ToString()));
        }

        int playerCount = players.Count;
        int totalRounds = playerCount - 1;
        int matchesPerRound = playerCount / 2;

        // 3. Algorytm Rotacji (Circle Method)
        for (int round = 1; round <= totalRounds; round++)
        {
            for (int i = 0; i < matchesPerRound; i++)
            {
                var home = players[i];
                var away = players[playerCount - 1 - i];

                // Pomin mecz, jeśli jeden z graczy to "Duch" (BYE)
                if (home.UserId != Guid.Empty.ToString() && away.UserId != Guid.Empty.ToString())
                {
                    // Naprzemienne przypisywanie ról Home/Away dla balansu
                    var match = (round + i) % 2 == 0 
                        ? new Match(tournamentId, home.UserId, away.UserId, round, tournamentId)
                        : new Match(tournamentId, away.UserId, home.UserId, round, tournamentId);
                    
                    allMatches.Add(match);
                }
                else
                {
                    // Opcjonalnie: Możesz zapisać mecz BYE w bazie z gotowym wynikiem
                    var byePlayerId = home.UserId == Guid.Empty.ToString() ? away.UserId : home.UserId;
                    var byeMatch = new Match(tournamentId, byePlayerId, Guid.Empty.ToString(), round, tournamentId);
                    byeMatch.SetResult(MatchResult.CreateBye());
                    allMatches.Add(byeMatch);
                }
            }

            // Rotacja: Stały pierwszy element, reszta przesuwa się w prawo
            // Przykład dla 4 graczy: 
            // R1: (1,4) (2,3) -> R2: (1,3) (4,2) -> R3: (1,2) (3,4)
            var lastPlayer = players[^1];
            players.RemoveAt(players.Count - 1);
            players.Insert(1, lastPlayer);
        }

        return allMatches;
    }
    }
}