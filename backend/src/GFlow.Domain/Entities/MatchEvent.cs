using System.Diagnostics.CodeAnalysis;

namespace GFlow.Domain.Entities
{
    public class MatchEvent
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public required string MatchId { get; set; }
        
        public required string EventType { get; set; } // "GOAL", "YELLOW_CARD", etc.
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public int? MinuteOfPlay { get; set; }
        
        public string? PlayerId { get; set; } // Player involved (optional)
        public string? Description { get; set; }
        public string? Metadata { get; set; } // JSON for sport-specific data
        
        public required string RecordedBy { get; set; } // UserId who recorded

        private MatchEvent() { }

        [SetsRequiredMembers]
        public MatchEvent(string matchId, string eventType, string recordedBy)
        {
            MatchId = matchId;
            EventType = eventType;
            RecordedBy = recordedBy;
        }
    }
}
