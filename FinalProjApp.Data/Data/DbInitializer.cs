using FinalProjApp.Models;
using FinalProjApp.Services;
using Microsoft.EntityFrameworkCore;

namespace FinalProjApp.Data
{
    /// <summary>Brings the database up to date and makes sure an admin can sign in.</summary>
    public static class DbInitializer
    {
        /// <param name="initialAdminPassword">Only used the first time, when there are no accounts yet.</param>
        public static void Initialize(DataContext db, string? initialAdminPassword)
        {
            db.Database.Migrate();

            if (db.Users.Any()) return;

            if (string.IsNullOrEmpty(initialAdminPassword))
                throw new InvalidOperationException(
                    "There are no accounts yet. Set Admin:InitialPassword (the Admin__InitialPassword app setting on Azure) to create the admin.");

            var salt = UserAccounts.GenerateSalt();
            db.Users.Add(new AppUser
            {
                Email = "admin@calybear.com",
                Salt = salt,
                PasswordHash = UserAccounts.HashPassword(initialAdminPassword, salt),
                Role = "Admin"
            });
            db.SaveChanges();
        }
    }
}
