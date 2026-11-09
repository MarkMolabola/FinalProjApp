using FinalProjApp.Models;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace FinalProjApp.Services
{
    /// <summary>
    /// The phone app's sign-in state. (The web app uses its cookie instead.)
    /// </summary>
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private ClaimsPrincipal _currentUser = new(new ClaimsIdentity());

        public override Task<AuthenticationState> GetAuthenticationStateAsync()
            => Task.FromResult(new AuthenticationState(_currentUser));

        public void NotifyUserLoggedIn(AppUser user)
        {
            SetCurrentUser(user);
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        protected void SetCurrentUser(AppUser user) => _currentUser = AuthClaims.CreatePrincipal(user, "CustomAuth");

        public void NotifyUserLoggedOut()
        {
            _currentUser = new ClaimsPrincipal(new ClaimsIdentity());
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
    }
}
