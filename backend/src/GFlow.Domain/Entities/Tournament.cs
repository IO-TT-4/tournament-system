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
}
}