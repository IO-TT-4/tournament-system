using System.Collections.Generic;

namespace GFlow.Application.DTOs
{
    public class TournamentListResponse
    {
        public List<TournamentResponse> Data { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int Limit { get; set; }
    }
}
