using Microsoft.AspNetCore.Components;

namespace FinalProjApp.Services
{
    /// <summary>
    /// Phone app sign-in: checks the password, then keeps the user in memory
    /// (<see cref="CustomAuthStateProvider"/>) for as long as the app runs.
    /// </summary>
    public class MauiAuthService : IAuthService
    {
        private readonly UserAccounts _accounts;
        private readonly CustomAuthStateProvider _authState;
        private readonly NavigationManager _nav;

        public MauiAuthService(UserAccounts accounts, CustomAuthStateProvider authState, NavigationManager nav)
        {
            _accounts = accounts;
            _authState = authState;
            _nav = nav;
        }

        public async Task<string?> SignInAsync(string email, string password)
        {
            var user = await _accounts.CheckPasswordAsync(email, password);
            if (user == null) return "That email and password don't match.";

            _authState.NotifyUserLoggedIn(user);
            _nav.NavigateTo(AuthClaims.LandingPage(user.Role));
            return null;
        }

        public Task SignOutAsync()
        {
            _authState.NotifyUserLoggedOut();
            _nav.NavigateTo("/login");
            return Task.CompletedTask;
        }
    }
}
