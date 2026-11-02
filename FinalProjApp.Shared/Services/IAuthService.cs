namespace FinalProjApp.Services
{
    /// <summary>
    /// Signing in and out. Each app does it its own way: the phone app keeps the
    /// user in memory, the web app sets a cookie so a page refresh keeps you in.
    /// Either way, on success the user ends up on <see cref="AuthClaims.LandingPage"/>.
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Returns null when signed in, otherwise a message to show. On success the user
        /// goes to <paramref name="returnUrl"/> (a page on this site) or their landing page.
        /// </summary>
        Task<string?> SignInAsync(string email, string password, string? returnUrl = null);

        Task SignOutAsync();
    }
}
