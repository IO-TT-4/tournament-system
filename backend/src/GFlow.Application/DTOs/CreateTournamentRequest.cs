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
    }
}