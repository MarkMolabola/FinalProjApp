using FinalProjApp.Data;
using FinalProjApp.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel.DataAnnotations;

namespace FinalProjApp.Services
{
    /// <summary>Contact messages in the database. Both the web pages and the API go through here.</summary>
    public class ContactService : IContactService
    {
        // The form needs no sign-in, so cap how much any one sender, and everyone together, can send
        private const int MaxPerEmailPerHour = 3;
        private const int MaxPerHour = 30;
        private const int MaxListed = 200;
        private static readonly object Lock = new();

        private readonly DataContext _db;
        private readonly IServiceScopeFactory _scopes;
        private readonly IMemoryCache _cache;

        public ContactService(DataContext db, IServiceScopeFactory scopes, IMemoryCache cache)
        {
            _db = db;
            _scopes = scopes;
            _cache = cache;
        }

        public event Action? Changed;

        public async Task SendAsync(ContactForm form)
        {
            var name = form.Name?.Trim() ?? "";
            var email = form.Email?.Trim() ?? "";
            var message = form.Message?.Trim() ?? "";

            if (name.Length == 0 || name.Length > ContactForm.MaxNameLength)
                throw new InvalidOperationException("Please enter your name.");
            if (email.Length > ContactForm.MaxEmailLength || !new EmailAddressAttribute().IsValid(email))
                throw new InvalidOperationException("Please enter a valid email address.");
            if (message.Length == 0)
                throw new InvalidOperationException("Please write a message.");
            if (message.Length > ContactForm.MaxMessageLength)
                throw new InvalidOperationException("Keep your message under 2000 characters.");

            if (!TryCount($"contact:{email.ToLowerInvariant()}", MaxPerEmailPerHour) || !TryCount("contact:all", MaxPerHour))
                throw new InvalidOperationException("We've received several messages already. Please try again later, or call us.");

            _db.ContactMessages.Add(new ContactMessage
            {
                Name = name,
                Email = email,
                Message = message,
                CreatedUtc = DateTime.UtcNow
            });
            await _db.SaveChangesAsync();
        }

        public Task<List<ContactMessage>> GetAsync(AppUser admin)
        {
            RequireAdmin(admin);
            return _db.ContactMessages.AsNoTracking()
                .OrderByDescending(m => m.Id)
                .Take(MaxListed)
                .ToListAsync();
        }

        public async Task<int> CountOpenAsync(AppUser admin)
        {
            RequireAdmin(admin);

            // The nav bar asks while pages load, and a DbContext can only do one thing at a time
            await using var scope = _scopes.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<DataContext>();
            return await db.ContactMessages.CountAsync(m => !m.IsHandled);
        }

        public async Task SetHandledAsync(AppUser admin, int messageId, bool handled)
        {
            RequireAdmin(admin);

            if (await _db.ContactMessages.FindAsync(messageId) is not { } message) return;
            message.IsHandled = handled;
            message.HandledUtc = handled ? DateTime.UtcNow : null;
            await _db.SaveChangesAsync();
            Changed?.Invoke();
        }

        /// <summary>Counts one more message for this key over the past hour; false once it's over the limit.</summary>
        private bool TryCount(string key, int limit)
        {
            lock (Lock)
            {
                var count = _cache.GetOrCreate(key, entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
                    return new Counter();
                })!;
                return ++count.Value <= limit;
            }
        }

        private static void RequireAdmin(AppUser user)
        {
            if (user.Role != "Admin")
                throw new InvalidOperationException("Only an admin can read contact messages.");
        }

        private sealed class Counter
        {
            public int Value;
        }
    }
}
