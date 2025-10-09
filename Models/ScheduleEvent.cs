using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProjApp.Models
{
    public class ScheduleEvent
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Subject { get; set; }
        public string Location { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public DateTime StartTime { get; set; }
        [Required]
        public DateTime EndTime { get; set; }
        public bool IsAllDay { get; set; }

        public int ChildId { get; set; } // Foreign key to Child


        public ScheduleEvent()
        {
            Subject = "Untitled Event";
            Description = "No description";
            StartTime = DateTime.Now;
            EndTime = DateTime.Now.AddHours(1);
            Location = string.Empty;
            IsAllDay = false;
        }



    }
}
