namespace GFlow.Application.DTOs
{
    public class MatchEventResponse
    {
        public required string Id { get; set; }
        public required string MatchId { get; set; }
        public required string EventType { get; set; }
        public DateTime Timestamp { get; set; }
        public int? MinuteOfPlay { get; set; }
        public string? PlayerId { get; set; }
        public string? PlayerName { get; set; }
        public string? Description { get; set; }
        public string? Metadata { get; set; }
        public required string RecordedBy { get; set; }
        public string? RecordedByName { get; set; }
    }
}
