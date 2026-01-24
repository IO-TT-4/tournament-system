using System;

namespace GFlow.Application.DTOs
{
    public class UpdateTournamentRequest
    {
        public string? Name { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? PlayerLimit { get; set; }
    }
}
