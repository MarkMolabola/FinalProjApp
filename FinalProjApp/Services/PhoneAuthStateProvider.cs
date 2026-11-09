using FinalProjApp.Models;
using FinalProjApp.Services.Api;
using Microsoft.AspNetCore.Components.Authorization;

namespace FinalProjApp.Services
{
    /// <summary>Puts back the saved sign-in the first time the app asks who's signed in.</summary>
    public class PhoneAuthStateProvider : CustomAuthStateProvider
    {
        private readonly ApiClient _api;
        private Task? _restore;

        public PhoneAuthStateProvider(ApiClient api) => _api = api;

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            await (_restore ??= RestoreAsync());
            return await base.GetAuthenticationStateAsync();
        }

        private async Task RestoreAsync()
        {
            if (await SavedSignIn.LoadAsync() is not { } login) return;

            _api.Token = login.Token;
            SetCurrentUser(new AppUser
            {
                UserId = login.UserId,
                Email = login.Email,
                Role = login.Role,
                GuardianId = login.GuardianId,
                PasswordHash = "",
                Salt = ""
            });
        }
    }
}
