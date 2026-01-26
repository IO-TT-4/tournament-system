using GFlow.Domain.ValueObjects;
using System.Diagnostics.CodeAnalysis;

namespace GFlow.Domain.Entities
{
    public class Match
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public required string PlayerHomeId { get; set; }
        public string? PlayerAwayId { get; set; }

        public int RoundNumber { get; set; }

        public required string TournamentId { get; set; }

        public int? PositionInRound { get; set; }

        public double? ScoreA { get; set; }
        public double? ScoreB { get; set; }
        public MatchFinishType? FinishType { get; set; }

        // Wrapper property to maintain backward compatibility with code using Result object
        public MatchResult? Result
        {
            get
            {
                if (ScoreA.HasValue && ScoreB.HasValue && FinishType.HasValue)
                {
                    return new MatchResult(ScoreA.Value, ScoreB.Value, FinishType.Value);
                }
                return null;
            }
            set
            {
                if (value == null)
                {
                    ScoreA = null;
                    ScoreB = null;
                    FinishType = null;
                }
                else
                {
                    ScoreA = value.ScoreA;
                    ScoreB = value.ScoreB;
                    FinishType = value.FinishType;
                }
            }
        }
        
        public List<MatchEvent> Events { get; set; } = new();

        private Match() { }

        [SetsRequiredMembers]
        public Match(string id, string playerAId, string? playerBId, int roundNumber, string tournamentId)
        {
            Id = id;
            PlayerHomeId = playerAId;
            PlayerAwayId = playerBId;
            RoundNumber = roundNumber;
            TournamentId = tournamentId;
        }

        public void FinishMatch(double scoreA, double scoreB)
        {
            Result = MatchResult.CreateNormal(scoreA, scoreB);
        }

        public void SetWalkover(string winnerId)
        {
            if (winnerId == PlayerHomeId)
                Result = MatchResult.CreateWalkoverForA();
            else if (winnerId == PlayerAwayId)
                Result = MatchResult.CreateWalkoverForB();
            else
                throw new ArgumentException("Winner must be one of the match participants.");
        }

        public void SetResult(MatchResult result)
        {
            Result = result;
        }

        public bool IsPlayerHome(string playerId)
        {
            return PlayerHomeId == playerId;
        }

        public bool IsCompleted => Result != null;
        
    }
}