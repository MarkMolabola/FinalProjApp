using System.Text.Json.Serialization;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProjApp.Models
{
    public class Child
    {
        public int ChildId { get; set; }
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        public DateTime DateofBirth { get; set; }
        public Guid GuardianID { get; set; } // Foreign key to Guardian
        [JsonIgnore] // navigation: not sent over the API
        public virtual ICollection<ScheduleEvent> Events { get; set; } = new List<ScheduleEvent>();
        [JsonIgnore]
        public virtual Guardian? Guardian { get; set; }



    }
}
