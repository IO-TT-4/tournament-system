using GFlow.Domain.ValueObjects;

namespace GFlow.Application.DTOs
{
    public class CreateTournamentRequest
    {
        public required string Name {get; set;}
        public required string OrganizerId {get; set;}
        public TournamentSystemType SystemType {get; set;}
        public int MaxParticipants {get; set;}
        public DateTime StartDate {get; set;}
        public DateTime EndDate {get; set;}
        
        // Location Details
        public string? CountryCode { get; set; }
        public string? City { get; set; }
        public string? Address { get; set; }
        public double? Lat { get; set; }
        public double? Lng { get; set; }

        // Game Details
        public string? GameCode { get; set; }
        public string? GameName { get; set; }

        public string Emblem {get; set;}
    }
}
