using GFlow.Domain.Entities;
using GFlow.Domain.ValueObjects;

namespace GFlow.Domain.Services.TieBreakers
{
    public class PointsDifferenceTieBreaker : ITieBreaker
    {
        public string Name => "Points Difference";
        public double Calculate(string userId, List<Match> allMatches, List<StandingsEntry> currentStandings)
        {
            double scored = allMatches.Where(m => m.PlayerHomeId == userId).Sum(m => m.Result?.ScoreA ?? 0)
                        + allMatches.Where(m => m.PlayerAwayId == userId).Sum(m => m.Result?.ScoreB ?? 0);
            
            double conceded = allMatches.Where(m => m.PlayerHomeId == userId).Sum(m => m.Result?.ScoreB ?? 0)
                        + allMatches.Where(m => m.PlayerAwayId == userId).Sum(m => m.Result?.ScoreA ?? 0);
            
            return scored - conceded;
        }
    }
}