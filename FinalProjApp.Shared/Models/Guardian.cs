using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace FinalProjApp.Models
{
    public class Guardian
    {
        [Key]
        [Required]
        public Guid GuardianId { get; set; }
        [Required]
        public string FullName { get; set; } = "";
        // Text, not a number: 10-digit numbers overflow an int and lose formatting.
        // Older databases still have an INTEGER column; SQLite stores this text there fine.
        [Required]
        [Phone(ErrorMessage = "Enter a valid phone number.")]
        [MaxLength(25)]
        public string PhoneNumber { get; set; } = "";
        [Required]
        public string Address { get; set; } = "";

        /// <summary>Where the parent-portal invite goes; also how siblings find the same guardian.</summary>
        [EmailAddress]
        [MaxLength(256)]
        public string? Email { get; set; }

        [JsonIgnore] // navigation: not sent over the API
        public virtual ICollection<Child> Children { get; set; } = new List<Child>();
    }
}
