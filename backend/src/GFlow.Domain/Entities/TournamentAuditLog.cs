using System.ComponentModel.DataAnnotations;

namespace GFlow.Domain.Entities
{
    /// <summary>
    /// General audit log for tournament management actions.
    /// </summary>
    public class TournamentAuditLog
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public required string TournamentId { get; set; }

        /// <summary>
        /// Action type: RegistrationRequest, RegistrationApproved, RegistrationRejected,
        /// ParticipantAdded, ParticipantRemoved, ParticipantWithdrawn, ParticipantMarkedWithdrawn,
        /// RoundPaired, MatchResultSubmitted
        /// </summary>
        public required string ActionType { get; set; }

        /// <summary>
        /// The user who was affected by the action (e.g. participant added/removed).
        /// </summary>
        public string? TargetUserId { get; set; }
        public string? TargetUsername { get; set; }

        /// <summary>
        /// The user who performed the action.
        /// </summary>
        public required string PerformedById { get; set; }
        public string? PerformedByUsername { get; set; }

        /// <summary>
        /// Optional extra details (JSON or text).
        /// </summary>
        public string? Details { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
