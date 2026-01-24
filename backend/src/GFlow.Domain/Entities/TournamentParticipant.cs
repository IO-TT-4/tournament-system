using System.Diagnostics.CodeAnalysis;

namespace GFlow.Domain.ValueObjects
{
    public class TournamentParticipant
    {
        public required string UserId { get; set; }
        public double Score { get; set; }
        public double Ranking { get; set; }
        public bool HasReceivedBye { get; set; }
        public List<string> PlayedOpponentIds { get; } = new();
        public List<bool> RoleHistory { get; } = new();
        public required string TournamentId { get; set; }
        
        // NEW: History of score differences with opponents (for rules 6-7)
        public List<double> OpponentScoreHistory { get; } = new();
        
        // NEW: Withdrawal and Unavailability logic
        public bool IsWithdrawn { get; set; }
        public List<int> UnavailableRounds { get; } = new();

        public int RoleBalance => RoleHistory.Count(r => r) - RoleHistory.Count(r => !r);

        private TournamentParticipant() { }

        [SetsRequiredMembers]
        public TournamentParticipant(string id, double ranking = 1000)
        {
            UserId = id;
            Score = 0;
            Ranking = ranking;
            TournamentId = string.Empty;
            IsWithdrawn = false;
        }
        
        [SetsRequiredMembers]
        public TournamentParticipant(TournamentParticipant other)
        {
            UserId = other.UserId;
            Score = other.Score;
            Ranking = other.Ranking;
            HasReceivedBye = other.HasReceivedBye;
            PlayedOpponentIds = new List<string>(other.PlayedOpponentIds);
            RoleHistory = new List<bool>(other.RoleHistory);
            OpponentScoreHistory = new List<double>(other.OpponentScoreHistory);
            TournamentId = other.TournamentId;
            IsWithdrawn = other.IsWithdrawn;
            UnavailableRounds = new List<int>(other.UnavailableRounds);
        }
    }
}