namespace GFlow.Domain.ValueObjects
{
    public class MatchResult
    {
        public double ScoreA { get; set; }
        public double ScoreB { get; set; }
        public MatchFinishType FinishType { get; set; }

        public MatchResult() { }

        public MatchResult(double scoreA, double scoreB, MatchFinishType finishType = MatchFinishType.Normal)
        {
            if (scoreA < 0 || scoreB < 0)
                throw new ArgumentException("Scores cannot be negative.");

            ScoreA = scoreA;
            ScoreB = scoreB;
            FinishType = finishType;
        }

        public bool IsRatedGame => FinishType == MatchFinishType.Normal;

        public static MatchResult CreateNormal(double scoreA, double scoreB) 
        => new(scoreA, scoreB, MatchFinishType.Normal);

    public static MatchResult CreateWalkoverForA() 
        => new(1.0, 0.0, MatchFinishType.Walkover);

    public static MatchResult CreateWalkoverForB() 
        => new(0.0, 1.0, MatchFinishType.Walkover);
        
    public static MatchResult CreateBye() 
        => new(1.0, 0.0, MatchFinishType.Bye);
    }
}