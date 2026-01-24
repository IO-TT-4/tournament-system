using System;

namespace GFlow.Domain.Entities
{
    public class UserActivity
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public required string UserId { get; set; }
        public required string TournamentId { get; set; }
        public required string ActivityType { get; set; } // View, Join, Click
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public User User { get; set; } = null!;
        public Tournament Tournament { get; set; } = null!;
    }
}
