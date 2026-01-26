namespace GFlow.Application.DTOs
{
    public class StandingsEntry
    {
        public required string UserId { get; set; }
        public string? Username { get; set; } // Optional, populated if user data available
        public double Score { get; set; }
        public double Ranking { get; set; }
        public int MatchesPlayed { get; set; }
        public int Wins { get; set; }
        public int Draws { get; set; }
        public int Losses { get; set; }
        public bool IsWithdrawn { get; set; }
        public double Buchholz { get; set; }
        public Dictionary<string, double> TieBreakerValues { get; set; } = new();
    }
}
