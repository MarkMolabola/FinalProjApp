using System.ComponentModel.DataAnnotations;

namespace FinalProjApp.Models
{
    public static class ScheduleRequestStatus
    {
        public const string Pending = "Pending";
        public const string Approved = "Approved";
        public const string Declined = "Declined";
    }

    /// <summary>A schedule a parent has asked for. It becomes a ScheduleEvent once the admin confirms it.</summary>
    public class ScheduleRequest
    {
        [Key]
        public int Id { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Choose a kid.")]
        public int ChildId { get; set; }

        [Required]
        public DateTime StartTime { get; set; } = DateTime.Today.AddDays(1).AddHours(8);
        [Required]
        public DateTime EndTime { get; set; } = DateTime.Today.AddDays(1).AddHours(17);

        public bool NeedsPickup { get; set; }
        public string? RecurrenceRule { get; set; }

        [MaxLength(500, ErrorMessage = "Keep the note under 500 characters.")]
        public string? Note { get; set; }

        // Filled in by the server, never taken from what the parent sends
        public string ChildName { get; set; } = "";
        public int RequestedByUserId { get; set; }
        public string RequestedByEmail { get; set; } = "";
        public DateTime CreatedUtc { get; set; }
        public string Status { get; set; } = ScheduleRequestStatus.Pending;
        public DateTime? ReviewedUtc { get; set; }
        public string? ReviewedBy { get; set; }
        public string? ReviewNote { get; set; }
    }

    /// <summary>POST /api/schedule-requests/{id}/decline body.</summary>
    public class DeclineScheduleRequest
    {
        public string? Reason { get; set; }
    }
}
