using System.Reflection;
using GFlow.Domain.Models;

namespace GFlow.Application.Services
{
    public class TournamentService
    {
        Dictionary<int, Tournament> currentTournaments = BuildDictionary();
        Dictionary<int, Tournament> upcomingTournaments = BuildDictionaryUpcoming();

        public Tournament getTournament(int id)
        {
            return currentTournaments[id];
        }

        public List<Tournament> GetCurrentTournaments()
        {
            List<Tournament> _tournaments = new List<Tournament>();
            foreach (var item in currentTournaments)
            {
                _tournaments.Add(item.Value);
            }

            return _tournaments;
        }

        public List<Tournament> GetUpcomingTournaments()
        {
            List<Tournament> _tournaments = new List<Tournament>();
            foreach (var item in upcomingTournaments)
            {
                _tournaments.Add(item.Value);
            }

            return _tournaments;
        }

        private static Dictionary<int, Tournament> BuildDictionary() => new()
        {
            {
                1, new Tournament(1, "Tata Steel", "17.11.2025", "18.11.2025")
            },
            {
                2, new Tournament(2, "Tata Steel", "17.11.2025", "18.11.2025")
            },
            {
                3, new Tournament(3, "Tata Steel", "17.11.2025", "18.11.2025")
            },
        };

        private static Dictionary<int, Tournament> BuildDictionaryUpcoming() => new()
        {
            {
                4, new Tournament(4, "Tata Steel", "17.11.2025", "18.11.2025")
            },
            {
                5, new Tournament(5, "Tata Steel", "17.11.2025", "18.11.2025")
            },
            {
                6, new Tournament(6, "Tata Steel", "17.11.2025", "18.11.2025")
            },
        };
    }
}