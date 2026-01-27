using System.ComponentModel.DataAnnotations;

namespace GFlow.Domain.Entities
{
    public class MatchResultAudit
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public required string MatchId { get; set; }
        public required string TournamentId { get; set; }

        public double? OldScoreA { get; set; }
        public double? OldScoreB { get; set; }
        
        public double NewScoreA { get; set; }
        public double NewScoreB { get; set; }

        public required string ModifiedByDefaultId { get; set; }
        public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// e.g. "Initial Submission", "Correction", "Walkover"
        /// </summary>
        public string ChangeType { get; set; } = "Submission"; 
    }
}
