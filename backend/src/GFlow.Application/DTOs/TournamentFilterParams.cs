namespace GFlow.Application.DTOs
{
    public class TournamentFilterParams
    {
        public string? SearchTerm { get; set; }
        public string? GameCode { get; set; }
        public string? Status { get; set; }
        public string? City { get; set; }
        public double? Lat { get; set; }
        public double? Lng { get; set; }
        public double? Radius { get; set; } // km
        public string? SortBy { get; set; } // relevance, date-asc, date-desc, popular
        public int Page { get; set; } = 1;
        public int Limit { get; set; } = 20;
        
        // Internal fields for processing
        public string? UserIp { get; set; }
        public string? UserId { get; set; }
    }
}
