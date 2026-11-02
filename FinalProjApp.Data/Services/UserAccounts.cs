using FinalProjApp.Data;
using FinalProjApp.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace FinalProjApp.Services
{
    /// <summary>User accounts: checks passwords and creates accounts. Signing in is each app's IAuthService.</summary>
    public class UserAccounts
    {
        private readonly DataContext _db;

        public UserAccounts(DataContext db)
        {
            _db = db;
        }

        /// <summary>The user with this email and password, or null.</summary>
        public async Task<AppUser?> CheckPasswordAsync(string email, string password)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email.ToLower());
            if (user == null) return null;

            return HashPassword(password, user.Salt) == user.PasswordHash ? user : null;
        }

        public async Task<bool> RegisterAsync(string email, string password, string role, Guid? guardianId = null)
        {
            if (await _db.Users.AnyAsync(u => u.Email == email.ToLower()))
                return false;

            var salt = GenerateSalt();
            var user = new AppUser
            {
                Email = email.ToLower(),
                Salt = salt,
                PasswordHash = HashPassword(password, salt),
                Role = role,
                GuardianId = guardianId
            };

            await _db.Users.AddAsync(user);
            await _db.SaveChangesAsync();
            return true;
        }

        public static string GenerateSalt()
        {
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(16));
        }

        public static string HashPassword(string password, string salt)
        {
            return Convert.ToBase64String(
                SHA256.HashData(Encoding.UTF8.GetBytes(password + salt)));
        }
    }
}
