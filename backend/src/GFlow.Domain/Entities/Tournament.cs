using GFlow.Domain.ValueObjects;

namespace GFlow.Domain.Entities
{
    public class Tournament
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public required string Name { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int PlayerLimit { get; set; }
    public TournamentStatus Status { get; set; }
    public TournamentSystemType SystemType { get; set; }
    public string? OrganizerId { get; set; }
    public User Organizer { get; set; } = null!;
    public List<User> Participants { get; set; } = new();
    public List<User> Moderators { get; set; } = new();
    
    /// <summary>
    /// How players should be seeded/paired in the first round (Swiss/Elimination).
    /// </summary>
    public SeedingType SeedingType { get; set; } = SeedingType.Random;
    
    /// <summary>
    /// List of Tie Breaker codes (e.g., "BUCHHOLZ", "SONNEBORN_BERGER", "DIRECT_MATCH").
    /// Order matters: first item is the primary tie breaker.
    /// </summary>
    public List<string> TieBreakers { get; set; } = new();
    
    // Location Details
    public string? CountryCode { get; set; }
    public string? City { get; set; }
    public string? Address { get; set; }
    public double? Lat { get; set; }
    public double? Lng { get; set; }

    // Game Details (to match frontend)
    public string? GameCode { get; set; }
    public string? GameName { get; set; }
    public string? Emblem { get; set; }
    public string? Description { get; set; }
    public int? NumberOfRounds { get; set; }

    // Statistics
    public long ViewCount { get; set; }
}


}