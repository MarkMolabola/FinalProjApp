using System.ComponentModel.DataAnnotations;

namespace FinalProjApp.Models
{
    /// <summary>
    /// Everything the Add / Edit kid form collects, child and guardian together, so
    /// one EditForm validates every field before anything is saved.
    /// </summary>
    public class KidForm
    {
        [Required(ErrorMessage = "Enter the kid's first name.")]
        [StringLength(50, ErrorMessage = "Keep the first name under 50 characters.")]
        public string FirstName { get; set; } = "";

        [Required(ErrorMessage = "Enter the kid's last name.")]
        [StringLength(50, ErrorMessage = "Keep the last name under 50 characters.")]
        public string LastName { get; set; } = "";

        [Required(ErrorMessage = "Enter a date of birth.")]
        [PastDate(MaxYears = 13)]
        public DateTime DateofBirth { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Enter the guardian's name.")]
        [StringLength(100, ErrorMessage = "Keep the name under 100 characters.")]
        public string GuardianName { get; set; } = "";

        // 10 digits (optionally a leading 1) with any of ( ) - . + spaces between them
        [Required(ErrorMessage = "Enter a phone number.")]
        [RegularExpression(@"^[\s().+-]*(1[\s().-]*)?(\d[\s().-]*){10}$",
            ErrorMessage = "Enter a 10-digit phone number, like (555) 123-4567.")]
        public string Phone { get; set; } = "";

        [Required(ErrorMessage = "Enter an address.")]
        [StringLength(200, ErrorMessage = "Keep the address under 200 characters.")]
        public string Address { get; set; } = "";

        public static KidForm From(Child child, Guardian? guardian) => new()
        {
            FirstName = child.FirstName,
            LastName = child.LastName,
            DateofBirth = child.DateofBirth,
            GuardianName = guardian?.FullName ?? "",
            Phone = guardian?.PhoneNumber ?? "",
            Address = guardian?.Address ?? ""
        };
    }

    /// <summary>A birth date that isn't in the future and isn't more than <see cref="MaxYears"/> years ago.</summary>
    public class PastDateAttribute : ValidationAttribute
    {
        public int MaxYears { get; set; } = 13;

        protected override ValidationResult? IsValid(object? value, ValidationContext context)
        {
            if (value is not DateTime date) return ValidationResult.Success;
            if (date.Date > DateTime.Today)
                return new ValidationResult("Date of birth can't be in the future.", new[] { context.MemberName! });
            if (date.Date < DateTime.Today.AddYears(-MaxYears))
                return new ValidationResult($"That's over {MaxYears} years ago. Double-check the year.", new[] { context.MemberName! });
            return ValidationResult.Success;
        }
    }
}
