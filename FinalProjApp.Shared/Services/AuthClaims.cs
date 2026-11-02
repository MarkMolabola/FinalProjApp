using FinalProjApp.Models;
using System.Security.Claims;

namespace FinalProjApp.Services
{
    /// <summary>How a signed-in user is stored as claims, shared by both apps' sign-in.</summary>
    public static class AuthClaims
    {
        public const string UserIdClaim = "UserId";
        public const string GuardianIdClaim = "GuardianId";

        public static ClaimsPrincipal CreatePrincipal(AppUser user, string authenticationType)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, user.Email),
                new(ClaimTypes.Role, user.Role),
                new(UserIdClaim, user.UserId.ToString())
            };

            if (user.GuardianId.HasValue)
                claims.Add(new Claim(GuardianIdClaim, user.GuardianId.Value.ToString()));

            return new ClaimsPrincipal(new ClaimsIdentity(claims, authenticationType));
        }

        /// <summary>The signed-in user (no password data), or null when signed out.</summary>
        public static AppUser? ToAppUser(this ClaimsPrincipal principal)
        {
            if (principal.Identity?.IsAuthenticated != true) return null;

            var guardianId = principal.FindFirst(GuardianIdClaim)?.Value;
            return new AppUser
            {
                Email = principal.FindFirst(ClaimTypes.Name)?.Value ?? "",
                Role = principal.FindFirst(ClaimTypes.Role)?.Value ?? "",
                UserId = int.TryParse(principal.FindFirst(UserIdClaim)?.Value, out var id) ? id : 0,
                GuardianId = Guid.TryParse(guardianId, out var g) ? g : null,
                PasswordHash = "",
                Salt = ""
            };
        }

        /// <summary>Where each role lands after signing in.</summary>
        public static string LandingPage(string role) => role == "Parent" ? "/parent-portal" : "/";
    }
}
