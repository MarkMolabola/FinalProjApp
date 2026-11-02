using System;
using System.ComponentModel.DataAnnotations;

namespace FinalProjApp.Models
{
    public class ScheduleEvent
    {
        [Key]
        public int Id { get; set; }

        // Auto-set to child's name before saving
        public string Subject { get; set; } = "Event";

        // Legacy field kept for seed data
        public string? Description { get; set; }
        public string? Location { get; set; }

        // New fields
        public string? Note { get; set; }
        public bool NeedsPickup { get; set; } = false;

        // Syncfusion RRULE string (e.g. "FREQ=WEEKLY;INTERVAL=1;")
        public string? RecurrenceRule { get; set; }
        public int? RecurrenceID { get; set; }
        public string? RecurrenceException { get; set; }

        [Required]
        public DateTime StartTime { get; set; } = DateTime.Now;
        [Required]
        public DateTime EndTime { get; set; } = DateTime.Now.AddHours(1);
        public bool IsAllDay { get; set; } = false;

        public int ChildId { get; set; }
    }
}
