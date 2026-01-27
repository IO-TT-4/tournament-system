namespace GFlow.Application.DTOs
{
    public class TournamentResponse
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required string OrganizerName { get; set; }
    public int PlayerLimit { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int? NumberOfRounds { get; set; }
    public List<string> TieBreakers { get; set; } = new();
    
    // New fields
    public string? Status { get; set; }
    public string? GameCode { get; set; }
    public string? GameName { get; set; }
    public string? City { get; set; }
    public string? Address { get; set; }
    public double? Lat { get; set; }
    public double? Lng { get; set; }
    public long ViewCount { get; set; }
    public int ParticipantCount { get; set; }
    public double RelevanceScore { get; set; }
    public string? Emblem { get; set; }
    public required string SystemType { get; set; }
    public string? Description { get; set; }
    public required string OrganizerId { get; set; }
    public List<string> ModeratorIds { get; set; } = new();
    public List<ParticipantDto> Participants { get; set; } = new();
    
    public double? WinPoints { get; set; }
    public double? DrawPoints { get; set; }
    public double? LossPoints { get; set; }
    
    public string RegistrationMode { get; set; } = "Open";
    public bool EnableMatchEvents { get; set; } = false;
}

public class ParticipantDto
{
    public required string Id { get; set; }
    public required string Username { get; set; }
    public bool IsWithdrawn { get; set; }
    public string Status { get; set; } = "Confirmed";
}


}