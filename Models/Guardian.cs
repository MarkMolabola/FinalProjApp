using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProjApp.Models
{
    public class Guardian
    {
        [Key]
        [Required]
        public Guid GuardianId { get; set; }
        [Required]
        public string FullName { get; set; }
        [Required]
        public int PhoneNumber { get; set; }
        [Required]
        public string Address { get; set; }

        public virtual ObservableCollectionListSource<Child> Children { get; set; }

        public Guardian()
        {
            Children = new ObservableCollectionListSource<Child>();
        }

    }
}
