using FinalProjApp.Models;
using System.Security.Claims;

namespace FinalProjApp.Services
{
    /// <summary>How a signed-in user is stored as claims, shared by both apps' sign-in.</summary>
    public static class AuthClaims
    {
        public const string UserIdClaim = "UserId";
        public const string GuardianIdClaim = "GuardianId";
        public const string TooManyAttemptsMessage = "Too many sign-in attempts. Please wait 15 minutes and try again.";

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

        /// <summary>Where to go after signing in: the requested page if it's on this site, else the landing page.</summary>
        public static string AfterSignIn(string role, string? returnUrl) =>
            returnUrl is { Length: > 1 } url && url.StartsWith('/') && !url.StartsWith("//") && !url.StartsWith("/\\")
                && !url.StartsWith("/login", StringComparison.OrdinalIgnoreCase)
                ? url
                : LandingPage(role);
    }
}
