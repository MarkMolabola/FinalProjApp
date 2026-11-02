using System.ComponentModel.DataAnnotations;

namespace FinalProjApp.Models
{
    public class AppUser
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        [Required]
        public string Salt { get; set; }

        /// <summary>Admin, Staff, Parent</summary>
        [Required]
        public string Role { get; set; }

        /// <summary>Linked Guardian record for Parent role accounts.</summary>
        public Guid? GuardianId { get; set; }
    }
}
