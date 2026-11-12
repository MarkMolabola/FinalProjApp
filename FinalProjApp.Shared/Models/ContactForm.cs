using System.ComponentModel.DataAnnotations;

namespace FinalProjApp.Models
{
    public class ContactForm
    {
        public const int MaxNameLength = 100;
        public const int MaxEmailLength = 256;
        public const int MaxMessageLength = 2000;

        [Required, MaxLength(MaxNameLength)]
        public string Name { get; set; } = "";

        [Required, EmailAddress, MaxLength(MaxEmailLength)]
        public string Email { get; set; } = "";

        [Required, MaxLength(MaxMessageLength, ErrorMessage = "Keep your message under 2000 characters.")]
        public string Message { get; set; } = "";
    }
}
