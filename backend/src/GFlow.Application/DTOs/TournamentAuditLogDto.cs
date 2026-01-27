namespace GFlow.Application.DTOs
{
    public class TournamentAuditLogDto
    {
        public string Id { get; set; } = string.Empty;
        public string TournamentId { get; set; } = string.Empty;
        public string ActionType { get; set; } = string.Empty;
        public string? TargetUserId { get; set; }
        public string? TargetUsername { get; set; }
        public string PerformedById { get; set; } = string.Empty;
        public string? PerformedByUsername { get; set; }
        public string? Details { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
