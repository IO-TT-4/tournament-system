namespace GFlow.Domain.ValueObjects
{
    public class StandingsEntry
    {
        public int Position { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public double Points { get; set; }
        
        // Tie-breakery (Szczególnie ważne w Swiss i Round Robin)
        public double Buchholz { get; set; } // Suma punktów przeciwników (Swiss)
        public double DirectEncounter { get; set; } // Wynik bezpośredniego meczu
        public int WinsCount { get; set; }
        public int MatchesPlayed { get; set; }

        public Dictionary<string, double> TieBreakerValues { get; set; } = new();
    }
}