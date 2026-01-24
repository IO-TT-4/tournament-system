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
            if (match == null) return;

            var tournament = await _repository.GetTournament(match.TournamentId);
            if (tournament == null) return;
            
            // 1. Save result
            match.SetResult(MatchResult.CreateNormal(scoreA, scoreB));
            await _repository.UpdateMatch(match);

            // 2. Check if the round is finished
            var currentRoundMatches = await _repository.GetMatchesByRound(tournament.Id, match.RoundNumber);
            
            if (currentRoundMatches.All(m => m.IsCompleted))
            {
                // 3. If round is closed, try to generate the next one
                var participants = await _repository.GetParticipants(tournament.Id);
                var allMatches = await _repository.GetAllMatches(tournament.Id);
                
                var strategy = _strategyFactory.GetStrategy(tournament.SystemType);
                var nextRound = strategy.GenerateNextRound(tournament, participants, allMatches);

                if (nextRound.Any())
                {
                    await _repository.SaveMatches(nextRound);
                    // Here you can send a notification (e.g. SignalR) that the new round is ready
                }
            }
        }
    }
}