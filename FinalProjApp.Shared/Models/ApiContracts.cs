namespace FinalProjApp.Models
{
    /// <summary>POST /api/auth/login body.</summary>
    public class LoginRequest
    {
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
    }

    /// <summary>POST /api/auth/login reply: a token for the Authorization header, and who signed in.</summary>
    public class LoginResponse
    {
        public string Token { get; set; } = "";
        public DateTime ExpiresUtc { get; set; }
        public int UserId { get; set; }
        public string Email { get; set; } = "";
        public string Role { get; set; } = "";
        public Guid? GuardianId { get; set; }
    }
}
