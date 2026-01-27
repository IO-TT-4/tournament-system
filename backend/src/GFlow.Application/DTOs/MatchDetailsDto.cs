namespace GFlow.Application.DTOs
{
    public class MatchDetailsDto : MatchDto
    {
        public double ScoreA { get; set; }
        public double ScoreB { get; set; }
        public bool EnableMatchEvents { get; set; }
    }
}
