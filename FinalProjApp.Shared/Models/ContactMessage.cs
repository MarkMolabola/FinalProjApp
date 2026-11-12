using System.ComponentModel.DataAnnotations;

namespace FinalProjApp.Models
{
    /// <summary>A message someone sent from the Contact page, kept until the admin deals with it.</summary>
    public class ContactMessage
    {
        public int Id { get; set; }

        [MaxLength(ContactForm.MaxNameLength)]
        public string Name { get; set; } = "";

        [MaxLength(ContactForm.MaxEmailLength)]
        public string Email { get; set; } = "";

        [MaxLength(ContactForm.MaxMessageLength)]
        public string Message { get; set; } = "";

        public DateTime CreatedUtc { get; set; }

        public bool IsHandled { get; set; }
        public DateTime? HandledUtc { get; set; }
    }
}
