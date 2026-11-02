using FinalProjApp.Models;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace FinalProjApp.Services
{
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private ClaimsPrincipal _currentUser = new(new ClaimsIdentity());

        public override Task<AuthenticationState> GetAuthenticationStateAsync()
            => Task.FromResult(new AuthenticationState(_currentUser));

        public void NotifyUserLoggedIn(AppUser user)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, user.Email),
                new(ClaimTypes.Role, user.Role),
                new("UserId", user.UserId.ToString())
            };

            if (user.GuardianId.HasValue)
                claims.Add(new Claim("GuardianId", user.GuardianId.Value.ToString()));

            _currentUser = new ClaimsPrincipal(new ClaimsIdentity(claims, "CustomAuth"));
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        public void NotifyUserLoggedOut()
        {
            _currentUser = new ClaimsPrincipal(new ClaimsIdentity());
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        public AppUser? GetCurrentUserInfo()
        {
            if (_currentUser.Identity == null || !_currentUser.Identity.IsAuthenticated)
                return null;

            var guidStr = _currentUser.FindFirst("GuardianId")?.Value;
            return new AppUser
            {
                Email = _currentUser.FindFirst(ClaimTypes.Name)?.Value ?? "",
                Role = _currentUser.FindFirst(ClaimTypes.Role)?.Value ?? "",
                UserId = int.Parse(_currentUser.FindFirst("UserId")?.Value ?? "0"),
                GuardianId = guidStr != null ? Guid.Parse(guidStr) : null
            };
        }
    }
}
