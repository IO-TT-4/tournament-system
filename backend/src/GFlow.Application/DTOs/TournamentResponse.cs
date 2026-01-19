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
    public int? numberOfRounds { get; set; }
}
}