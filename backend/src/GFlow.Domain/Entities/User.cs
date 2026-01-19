namespace GFlow.Domain.Entities
{
    public class User
    {
        public required string PasswordHash {get; set;}
        
        public string? Token {get; set;}

        public string Id { get; set; } = Guid.NewGuid().ToString();
        public required string Username { get; set; }
        public string? Email { get; set; }

        // Relacje
        public List<Tournament> OrganizedTournaments { get; set; } = new();
        public List<Tournament> ParticipatedTournaments { get; set; } = new();
    }
}