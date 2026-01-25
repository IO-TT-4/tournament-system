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
}


}