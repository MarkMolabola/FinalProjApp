using FinalProjApp.Models;
using FinalProjApp.Services.Api;
using Microsoft.AspNetCore.Components;
using System.Net;
using System.Net.Http.Json;

namespace FinalProjApp.Services
{
    /// <summary>
    /// Phone app sign-in: the web app checks the password and returns a token, which
    /// every API call then carries. The user is kept in memory
    /// (<see cref="CustomAuthStateProvider"/>) for as long as the app runs.
    /// </summary>
    public class MauiAuthService : IAuthService
    {
        private readonly ApiClient _api;
        private readonly CustomAuthStateProvider _authState;
        private readonly NavigationManager _nav;

        public MauiAuthService(ApiClient api, CustomAuthStateProvider authState, NavigationManager nav)
        {
            _api = api;
            _authState = authState;
            _nav = nav;
        }

        public async Task<string?> SignInAsync(string email, string password)
        {
            HttpResponseMessage response;
            try
            {
                response = await _api.PostAsync("api/auth/login", new LoginRequest { Email = email, Password = password });
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                return "Can't reach Caly Bear right now. Check your connection and try again.";
            }

            if (response.StatusCode == HttpStatusCode.Unauthorized) return "That email and password don't match.";
            if (!response.IsSuccessStatusCode) return "Something went wrong signing in. Please try again.";

            var login = (await response.Content.ReadFromJsonAsync<LoginResponse>())!;
            _api.Token = login.Token;
            _authState.NotifyUserLoggedIn(new AppUser
            {
                UserId = login.UserId,
                Email = login.Email,
                Role = login.Role,
                GuardianId = login.GuardianId,
                PasswordHash = "",
                Salt = ""
            });
            _nav.NavigateTo(AuthClaims.LandingPage(login.Role));
            return null;
        }

        public Task SignOutAsync()
        {
            _api.Token = null;
            _authState.NotifyUserLoggedOut();
            _nav.NavigateTo("/login");
            return Task.CompletedTask;
        }
    }
}
