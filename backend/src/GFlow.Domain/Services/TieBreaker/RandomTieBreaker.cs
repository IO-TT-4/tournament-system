using GFlow.Domain.Entities;
using GFlow.Domain.ValueObjects;

namespace GFlow.Domain.Services.TieBreakers
{
    public class RandomTieBreaker : ITieBreaker
    {
        public string Name => "Random Draw";
        public double Calculate(string userId, List<Match> allMatches, List<StandingsEntry> currentStandings)
        {
            // Używamy HashCode z ID, aby wynik był stały dla tego samego usera w danym momencie
            return userId.GetHashCode();
        }
    }
}