using System;

using GFlow.Domain.ValueObjects;
using System.ComponentModel.DataAnnotations; // Assuming needed or just remove if not

namespace GFlow.Application.DTOs
{
    public class UpdateTournamentRequest
    {
        public string? Name { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? PlayerLimit { get; set; }
        public string? Emblem { get; set; }
        public double? WinPoints { get; set; }
        public double? DrawPoints { get; set; }
        public double? LossPoints { get; set; }
        public RegistrationMode? RegistrationMode { get; set; }
        public string? Description { get; set; }
        public bool? EnableMatchEvents { get; set; }
    }
}
