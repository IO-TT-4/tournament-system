using GFlow.Application.Ports;
using GFlow.Domain.ValueObjects;

namespace GFlow.Domain.Services
{
    public class TournamentFlowService
    {
        private readonly IPairingStrategyFactory _strategyFactory;
        private readonly ITournamentRepository _repository;

        public TournamentFlowService(IPairingStrategyFactory strategyFactory, ITournamentRepository repository)
        {
            _strategyFactory = strategyFactory;
            _repository = repository;
        }

        public async Task UpdateMatchResult(string matchId, double scoreA, double scoreB)
        {
            var match = await _repository.GetMatchById(matchId);
            var tournament = await _repository.GetTournament(match.TournamentId);
            
            // 1. Zapisz wynik
            match.SetResult(MatchResult.CreateNormal(scoreA, scoreB));
            await _repository.UpdateMatch(match);

            // 2. Sprawdź, czy runda się skończyła
            var currentRoundMatches = await _repository.GetMatchesByRound(tournament.Id, match.RoundNumber);
            
            if (currentRoundMatches.All(m => m.IsCompleted))
            {
                // 3. Jeśli runda zamknięta, spróbuj wygenerować następną
                var participants = await _repository.GetParticipants(tournament.Id);
                var allMatches = await _repository.GetAllMatches(tournament.Id);
                
                var strategy = _strategyFactory.GetStrategy(tournament.SystemType);
                var nextRound = strategy.GenerateNextRound(tournament.Id, participants, allMatches);

                if (nextRound.Any())
                {
                    await _repository.SaveMatches(nextRound);
                    // Tutaj możesz wysłać powiadomienie (np. SignalR), że nowa runda jest gotowa
                }
            }
        }
    }
}