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
        public int Id { get; set; }
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        public DateTime DateofBirth { get; set; }
        public Guid GuardianID { get; set; }

        public virtual Guardian Guardian { get; set; }

    }
}
