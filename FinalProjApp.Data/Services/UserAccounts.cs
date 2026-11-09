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
        private const string Pbkdf2Tag = "pbkdf2";
        private const int Iterations = 600_000;
        private const int HashBytes = 32;

        private readonly DataContext _db;

        public UserAccounts(DataContext db)
        {
            _db = db;
        }

        /// <summary>The user with this email and password, or null.</summary>
        public async Task<AppUser?> CheckPasswordAsync(string email, string password)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email.ToLower());
            if (user == null)
            {
                // Take as long as a real check, so timing doesn't reveal which emails have accounts
                HashPassword(password, "no-such-account");
                return null;
            }

            if (!Matches(password, user.Salt, user.PasswordHash)) return null;

            // Accounts from before the slow hash get upgraded the first time they sign in
            if (!user.PasswordHash.StartsWith(Pbkdf2Tag + "$"))
            {
                user.PasswordHash = HashPassword(password, user.Salt);
                await _db.SaveChangesAsync();
            }

            return user;
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

        /// <summary>A slow salted hash, stored as pbkdf2$iterations$hash so the cost can be raised later.</summary>
        public static string HashPassword(string password, string salt)
        {
            return $"{Pbkdf2Tag}${Iterations}${Pbkdf2(password, salt, Iterations)}";
        }

        private static string Pbkdf2(string password, string salt, int iterations)
        {
            return Convert.ToBase64String(Rfc2898DeriveBytes.Pbkdf2(
                password, Encoding.UTF8.GetBytes(salt), iterations, HashAlgorithmName.SHA256, HashBytes));
        }

        /// <summary>The single SHA-256 used before the slow hash; only for checking older accounts.</summary>
        private static string LegacyHash(string password, string salt)
        {
            return Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(password + salt)));
        }

        private static bool Matches(string password, string salt, string stored)
        {
            var parts = stored.Split('$');
            var computed = parts.Length == 3 && parts[0] == Pbkdf2Tag && int.TryParse(parts[1], out var iterations)
                ? $"{Pbkdf2Tag}${iterations}${Pbkdf2(password, salt, iterations)}"
                : LegacyHash(password, salt);

            return CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(computed), Encoding.UTF8.GetBytes(stored));
        }
    }
}
