using FinalProjApp.Data;
using FinalProjApp.Models;
using FinalProjApp.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace FinalProjApp.Web.Services
{
    /// <summary>
    /// Parent-portal invites: an emailed link to /register?code=…, good for 7 days and
    /// once only. The parent picks a password and their account is linked to their
    /// guardian record, so they see their kids.
    /// </summary>
    public class InviteService : IInviteService
    {
        private static readonly TimeSpan Lifetime = TimeSpan.FromDays(7);
        public const int MinPasswordLength = 8;

        private readonly DataContext _db;
        private readonly UserAccounts _accounts;
        private readonly IEmailSender _email;
        private readonly IConfiguration _config;
        private readonly IHttpContextAccessor _http;
        private readonly IServiceProvider _services;

        public InviteService(DataContext db, UserAccounts accounts, IEmailSender email, IConfiguration config,
            IHttpContextAccessor http, IServiceProvider services)
        {
            _db = db;
            _accounts = accounts;
            _email = email;
            _config = config;
            _http = http;
            _services = services;
        }

        public async Task<InviteResult> SendAsync(Guid guardianId)
        {
            var guardian = await _db.guardian.FindAsync(guardianId);
            if (guardian == null) return new InviteResult { Message = "That guardian wasn't found." };
            if (string.IsNullOrEmpty(guardian.Email))
                return new InviteResult { Message = $"Add an email for {guardian.FullName} first." };
            if (await _db.Users.AnyAsync(u => u.GuardianId == guardianId))
                return new InviteResult { Message = $"{guardian.FullName} already has an account." };

            // Only the newest invite works
            var now = DateTime.UtcNow;
            foreach (var old in await _db.Invites.Where(i => i.GuardianId == guardianId && i.UsedUtc == null).ToListAsync())
                old.UsedUtc = now;

            var code = Convert.ToHexString(RandomNumberGenerator.GetBytes(32)).ToLowerInvariant();
            _db.Invites.Add(new Invite
            {
                GuardianId = guardianId,
                Email = guardian.Email,
                CodeHash = Hash(code),
                CreatedUtc = now,
                ExpiresUtc = now.Add(Lifetime)
            });
            await _db.SaveChangesAsync();

            var link = $"{BaseUrl()}register?code={code}";
            var kids = await KidNamesAsync(guardianId);
            var sent = await _email.SendAsync(guardian.Email, "You're invited to Caly Bear 🧸", EmailBody(guardian.FullName, kids, link));

            return sent
                ? new InviteResult { Sent = true, Message = $"Invite sent to {guardian.Email}." }
                : new InviteResult
                {
                    Message = $"Email isn't set up yet. Copy this link and send it to {guardian.FullName}:",
                    Link = link
                };
        }

        public async Task<InviteDetails?> GetAsync(string code)
        {
            var invite = await FindValidAsync(code);
            if (invite == null) return null;

            var guardian = await _db.guardian.FindAsync(invite.GuardianId);
            return new InviteDetails
            {
                Email = invite.Email,
                GuardianName = guardian?.FullName ?? "",
                KidNames = await KidNamesAsync(invite.GuardianId)
            };
        }

        public async Task<string?> AcceptAsync(string code, string password)
        {
            if (string.IsNullOrEmpty(password) || password.Length < MinPasswordLength)
                return $"Use at least {MinPasswordLength} characters for your password.";

            var invite = await FindValidAsync(code);
            if (invite == null) return "This invite link has expired or was already used. Ask the daycare to send a new one.";

            if (!await _accounts.RegisterAsync(invite.Email, password, "Parent", invite.GuardianId))
                return "An account with this email already exists. Sign in instead.";

            invite.UsedUtc = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return null;
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private async Task<Invite?> FindValidAsync(string? code)
        {
            if (string.IsNullOrWhiteSpace(code)) return null;
            var hash = Hash(code.Trim().ToLowerInvariant());
            var now = DateTime.UtcNow;
            return await _db.Invites.FirstOrDefaultAsync(i => i.CodeHash == hash && i.UsedUtc == null && i.ExpiresUtc > now);
        }

        private Task<List<string>> KidNamesAsync(Guid guardianId) =>
            _db.children
                .Where(c => _db.ChildGuardians.Any(l => l.GuardianId == guardianId && l.ChildId == c.ChildId))
                .OrderBy(c => c.FirstName)
                .Select(c => c.FirstName)
                .ToListAsync();

        private static string Hash(string code) =>
            Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(code)));

        /// <summary>The site's address: App:PublicUrl if set (Azure), otherwise wherever this request came in.</summary>
        private string BaseUrl()
        {
            if (_config["App:PublicUrl"] is { Length: > 0 } configured)
                return configured.TrimEnd('/') + "/";
            if (_http.HttpContext?.Request is { } request)
                return $"{request.Scheme}://{request.Host}{request.PathBase}/";
            try
            {
                return _services.GetRequiredService<NavigationManager>().BaseUri;
            }
            catch (InvalidOperationException)
            {
                return "http://localhost:5208/";
            }
        }

        private static string EmailBody(string guardianName, List<string> kids, string link)
        {
            var who = kids.Count switch
            {
                0 => "your family's",
                1 => $"{WebUtility.HtmlEncode(kids[0])}'s",
                _ => $"{WebUtility.HtmlEncode(string.Join(", ", kids.SkipLast(1)))} and {WebUtility.HtmlEncode(kids[^1])}'s"
            };
            return $"""
                <div style="font-family:Segoe UI,Arial,sans-serif;max-width:480px;margin:auto;color:#3D3551">
                  <h2 style="color:#E9695A">🧸 Caly Bear Child Care</h2>
                  <p>Hi {WebUtility.HtmlEncode(guardianName)},</p>
                  <p>You're invited to the Caly Bear parent portal, where you can see {who} schedule, pickups and photos.</p>
                  <p><a href="{link}" style="display:inline-block;background:#FF8A7A;color:#fff;padding:12px 22px;border-radius:999px;text-decoration:none;font-weight:bold">Set up my account</a></p>
                  <p style="color:#9289A8;font-size:13px">This link works once and expires in 7 days. If you weren't expecting it, you can ignore this email.</p>
                </div>
                """;
        }
    }
}
