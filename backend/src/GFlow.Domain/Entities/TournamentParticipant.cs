namespace GFlow.Domain.ValueObjects
{
    public class TournamentParticipant
    {
        public string UserId { get; set; }
        public double Score { get; set; }
        public double Ranking { get; set; }
        public bool HasReceivedBye { get; set; }
        public List<string> PlayedOpponentIds { get; } = new();
        public List<bool> RoleHistory { get; } = new();
        public string TournamentId { get; set; }
        
        // NOWE: Historia różnic punktowych z przeciwnikami (dla zasad 6-7)
        public List<double> OpponentScoreHistory { get; } = new();
        
        public int RoleBalance => RoleHistory.Count(r => r) - RoleHistory.Count(r => !r);

        private TournamentParticipant() { }

        public TournamentParticipant(string id, double ranking = 1000)
        {
            UserId = id;
            Score = 0;
            Ranking = ranking;
        }
        
        public TournamentParticipant(TournamentParticipant other)
        {
            UserId = other.UserId;
            Score = other.Score;
            Ranking = other.Ranking;
            HasReceivedBye = other.HasReceivedBye;
            PlayedOpponentIds = new List<string>(other.PlayedOpponentIds);
            RoleHistory = new List<bool>(other.RoleHistory);
            OpponentScoreHistory = new List<double>(other.OpponentScoreHistory);
        }
    }
}