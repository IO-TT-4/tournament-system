namespace GFlow.Application.DTOs
{
    public class AuditLogDto
    {
        public string Id { get; set; } = string.Empty;
        public string MatchId { get; set; } = string.Empty;
        public double? OldScoreA { get; set; }
        public double? OldScoreB { get; set; }
        public double NewScoreA { get; set; }
        public double NewScoreB { get; set; }
        public string ModifiedBy { get; set; } = string.Empty;
        public DateTime ModifiedAt { get; set; }
        public string ChangeType { get; set; } = string.Empty;
    }
}
