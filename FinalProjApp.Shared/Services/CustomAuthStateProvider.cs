using FinalProjApp.Models;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace FinalProjApp.Services
{
    /// <summary>
    /// The phone app's sign-in state: the user lives in memory for as long as the
    /// app runs. (The web app uses its cookie instead.)
    /// </summary>
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private ClaimsPrincipal _currentUser = new(new ClaimsIdentity());

        public override Task<AuthenticationState> GetAuthenticationStateAsync()
            => Task.FromResult(new AuthenticationState(_currentUser));

        public void NotifyUserLoggedIn(AppUser user)
        {
            _currentUser = AuthClaims.CreatePrincipal(user, "CustomAuth");
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        public void NotifyUserLoggedOut()
        {
            _currentUser = new ClaimsPrincipal(new ClaimsIdentity());
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
    }
}
