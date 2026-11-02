using System.ComponentModel.DataAnnotations;

namespace FinalProjApp.Models
{
    /// <summary>The kid's own details (used on its own when editing a kid).</summary>
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

        public static KidForm From(Child child) => new()
        {
            FirstName = child.FirstName,
            LastName = child.LastName,
            DateofBirth = child.DateofBirth
        };

        public Child ToChild() => new()
        {
            FirstName = FirstName.Trim(),
            LastName = LastName.Trim(),
            DateofBirth = DateofBirth
        };
    }

    /// <summary>
    /// Adding a new kid: the kid plus their first guardian, validated together so
    /// nothing is saved until every field is right.
    /// </summary>
    public class NewKidForm : KidForm
    {
        [Required(ErrorMessage = "Enter the guardian's name.")]
        [StringLength(100, ErrorMessage = "Keep the name under 100 characters.")]
        public string GuardianName { get; set; } = "";

        [Required(ErrorMessage = "Enter a phone number.")]
        [RegularExpression(GuardianForm.PhonePattern, ErrorMessage = GuardianForm.PhoneMessage)]
        public string Phone { get; set; } = "";

        [Required(ErrorMessage = "Enter an address.")]
        [StringLength(200, ErrorMessage = "Keep the address under 200 characters.")]
        public string Address { get; set; } = "";

        [EmailAddress(ErrorMessage = "Enter a valid email, like name@example.com.")]
        [StringLength(256)]
        public string? Email { get; set; }

        public Guardian ToGuardian() => new GuardianForm
        {
            FullName = GuardianName, Phone = Phone, Address = Address, Email = Email
        }.ToGuardian();
    }

    /// <summary>Adding or editing one guardian.</summary>
    public class GuardianForm
    {
        // 10 digits (optionally a leading 1) with any of ( ) - . + spaces between them
        public const string PhonePattern = @"^[\s().+-]*(1[\s().-]*)?(\d[\s().-]*){10}$";
        public const string PhoneMessage = "Enter a 10-digit phone number, like (555) 123-4567.";

        public Guid GuardianId { get; set; }

        [Required(ErrorMessage = "Enter the guardian's name.")]
        [StringLength(100, ErrorMessage = "Keep the name under 100 characters.")]
        public string FullName { get; set; } = "";

        [Required(ErrorMessage = "Enter a phone number.")]
        [RegularExpression(PhonePattern, ErrorMessage = PhoneMessage)]
        public string Phone { get; set; } = "";

        [Required(ErrorMessage = "Enter an address.")]
        [StringLength(200, ErrorMessage = "Keep the address under 200 characters.")]
        public string Address { get; set; } = "";

        [EmailAddress(ErrorMessage = "Enter a valid email, like name@example.com.")]
        [StringLength(256)]
        public string? Email { get; set; }

        public static GuardianForm From(Guardian g) => new()
        {
            GuardianId = g.GuardianId, FullName = g.FullName, Phone = g.PhoneNumber,
            Address = g.Address, Email = g.Email
        };

        public Guardian ToGuardian() => new()
        {
            GuardianId = GuardianId,
            FullName = FullName.Trim(),
            PhoneNumber = Phone.Trim(),
            Address = Address.Trim(),
            Email = string.IsNullOrWhiteSpace(Email) ? null : Email.Trim().ToLowerInvariant()
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
