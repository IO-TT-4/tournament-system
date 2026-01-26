using System.Collections.Generic;

namespace GFlow.Application.DTOs
{
    public class MatchDto
    {
        public string Id { get; set; }
        public string TournamentId { get; set; }
        public int RoundNumber { get; set; }
        public int TableNumber { get; set; }
        public string PlayerHomeId { get; set; }
        public string PlayerAwayId { get; set; }
        
        // Enrichment
        public string PlayerHomeName { get; set; }
        public string PlayerAwayName { get; set; }

        public MatchResultDto? Result { get; set; }
        public bool IsCompleted { get; set; }
    }

    public class MatchResultDto
    {
        public double ScoreA { get; set; }
        public double ScoreB { get; set; }
        public string FinishType { get; set; }
    }
}
