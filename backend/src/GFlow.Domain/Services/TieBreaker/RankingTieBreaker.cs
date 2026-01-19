using GFlow.Domain.Entities;
using GFlow.Domain.ValueObjects;

namespace GFlow.Domain.Services.TieBreakers
{
    public class RankingTieBreaker : ITieBreaker
    {
        private readonly List<TournamentParticipant> _participants;
        public string Name => "Initial Ranking";

        public RankingTieBreaker(List<TournamentParticipant> participants) => _participants = participants;

        public double Calculate(string userId, List<Match> allMatches, List<StandingsEntry> currentStandings)
        {
            return _participants.FirstOrDefault(p => p.UserId == userId)?.Ranking ?? 0;
        }
    }
}