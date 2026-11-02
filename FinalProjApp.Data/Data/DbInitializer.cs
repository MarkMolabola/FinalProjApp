using FinalProjApp.Models;
using FinalProjApp.Services;
using Microsoft.EntityFrameworkCore;

namespace FinalProjApp.Data
{
    /// <summary>Creates the database on first run and makes sure an admin can sign in.</summary>
    public static class DbInitializer
    {
        public static void Initialize(DataContext db)
        {
            db.Database.EnsureCreated();

            // EnsureCreated doesn't add tables to a database that already exists,
            // so older databases get these two created by hand.
            db.Database.ExecuteSqlRaw(@"
                CREATE TABLE IF NOT EXISTS Users (
                    UserId INTEGER PRIMARY KEY AUTOINCREMENT,
                    Email TEXT NOT NULL,
                    PasswordHash TEXT NOT NULL,
                    Salt TEXT NOT NULL,
                    Role TEXT NOT NULL,
                    GuardianId TEXT NULL
                );");

            db.Database.ExecuteSqlRaw(@"
                CREATE TABLE IF NOT EXISTS Photos (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    FileName TEXT NOT NULL,
                    ContentType TEXT NOT NULL,
                    Data BLOB NOT NULL
                );");

            if (!db.Users.Any())
            {
                var salt = UserAccounts.GenerateSalt();
                db.Users.Add(new AppUser
                {
                    Email = "admin@calybear.com",
                    Salt = salt,
                    PasswordHash = UserAccounts.HashPassword("Admin123!", salt),
                    Role = "Admin"
                });
                db.SaveChanges();
            }
        }
    }
}
