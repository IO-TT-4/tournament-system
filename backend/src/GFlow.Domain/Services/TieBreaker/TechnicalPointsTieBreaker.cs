using GFlow.Domain.Entities;
using GFlow.Domain.ValueObjects;

namespace GFlow.Domain.Services.TieBreakers
{
    public class TechnicalPointsTieBreaker : ITieBreaker
    {
        public string Name => "Technical Points";
        public double Calculate(string userId, List<Match> allMatches, List<StandingsEntry> currentStandings)
        {
            return allMatches.Where(m => m.PlayerHomeId == userId).Sum(m => m.Result?.ScoreA ?? 0)
                + allMatches.Where(m => m.PlayerAwayId == userId).Sum(m => m.Result?.ScoreB ?? 0);
        }
    }
}