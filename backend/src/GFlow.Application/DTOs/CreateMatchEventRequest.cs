namespace GFlow.Application.DTOs
{
    public class CreateMatchEventRequest
    {
        public required string EventType { get; set; }
        public int? MinuteOfPlay { get; set; }
        public string? PlayerId { get; set; }
        public string? Description { get; set; }
        public string? Metadata { get; set; }
    }
}
