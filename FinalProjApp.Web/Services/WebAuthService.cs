using FinalProjApp.Models;
using FinalProjApp.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Cryptography;

namespace FinalProjApp.Web.Services
{
    /// <summary>
    /// Web sign-in. Interactive pages run over a live connection and can't set
    /// cookies, so after the password checks out we hand the browser a one-time
    /// ticket and send it to /account/signin, which sets the cookie (see Program.cs).
    /// </summary>
    public class WebAuthService : IAuthService
    {
        private static readonly TimeSpan TicketLifetime = TimeSpan.FromMinutes(1);

        private readonly UserAccounts _accounts;
        private readonly IMemoryCache _tickets;
        private readonly NavigationManager _nav;

        public WebAuthService(UserAccounts accounts, IMemoryCache tickets, NavigationManager nav)
        {
            _accounts = accounts;
            _tickets = tickets;
            _nav = nav;
        }

        public async Task<string?> SignInAsync(string email, string password, string? returnUrl = null)
        {
            var user = await _accounts.CheckPasswordAsync(email, password);
            if (user == null) return "That email and password don't match.";

            var ticket = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
            _tickets.Set(TicketKey(ticket), user, TicketLifetime);
            var next = Uri.EscapeDataString(AuthClaims.AfterSignIn(user.Role, returnUrl));
            _nav.NavigateTo($"/account/signin?ticket={ticket}&returnUrl={next}", forceLoad: true);
            return null;
        }

        public Task SignOutAsync()
        {
            _nav.NavigateTo("/account/signout", forceLoad: true);
            return Task.CompletedTask;
        }

        /// <summary>The user a ticket was issued for; each ticket works once.</summary>
        public static AppUser? RedeemTicket(IMemoryCache tickets, string? ticket)
        {
            if (string.IsNullOrEmpty(ticket) || !tickets.TryGetValue(TicketKey(ticket), out AppUser? user))
                return null;
            tickets.Remove(TicketKey(ticket));
            return user;
        }

        private static string TicketKey(string ticket) => $"signin-ticket:{ticket}";
    }
}
