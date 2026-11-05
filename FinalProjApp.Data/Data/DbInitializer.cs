using FinalProjApp.Models;
using FinalProjApp.Services;
using Microsoft.EntityFrameworkCore;

namespace FinalProjApp.Data
{
    /// <summary>Creates the database on first run and makes sure an admin can sign in.</summary>
    public static class DbInitializer
    {
        private static void AddColumnIfMissing(DataContext db, string table, string column, string definition)
        {
            var conn = db.Database.GetDbConnection();
            if (conn.State != System.Data.ConnectionState.Open) conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = $"SELECT COUNT(*) FROM pragma_table_info('{table}') WHERE name = '{column}'";
            if (Convert.ToInt32(cmd.ExecuteScalar()) == 0)
                db.Database.ExecuteSqlRaw($"ALTER TABLE \"{table}\" ADD COLUMN \"{column}\" {definition}");
        }

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
                    Data BLOB NOT NULL,
                    IsPublic INTEGER NOT NULL DEFAULT 0
                );");

            // Guardians & invites (Nov 2026): new columns and tables for older databases
            AddColumnIfMissing(db, "Photos", "IsPublic", "INTEGER NOT NULL DEFAULT 0");
            AddColumnIfMissing(db, "guardian", "Email", "TEXT NULL");

            db.Database.ExecuteSqlRaw(@"
                CREATE TABLE IF NOT EXISTS ChildGuardians (
                    ChildId INTEGER NOT NULL,
                    GuardianId TEXT NOT NULL,
                    CONSTRAINT PK_ChildGuardians PRIMARY KEY (ChildId, GuardianId)
                );");

            // Every kid's original guardian becomes their first link
            db.Database.ExecuteSqlRaw(@"
                INSERT OR IGNORE INTO ChildGuardians (ChildId, GuardianId)
                SELECT ChildId, GuardianID FROM children;");

            db.Database.ExecuteSqlRaw(@"
                CREATE TABLE IF NOT EXISTS Invites (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    GuardianId TEXT NOT NULL,
                    Email TEXT NOT NULL,
                    CodeHash TEXT NOT NULL,
                    CreatedUtc TEXT NOT NULL,
                    ExpiresUtc TEXT NOT NULL,
                    UsedUtc TEXT NULL
                );");
            db.Database.ExecuteSqlRaw("CREATE INDEX IF NOT EXISTS IX_Invites_CodeHash ON Invites (CodeHash);");

            db.Database.ExecuteSqlRaw(@"
                CREATE TABLE IF NOT EXISTS ScheduleRequests (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    ChildId INTEGER NOT NULL,
                    StartTime TEXT NOT NULL,
                    EndTime TEXT NOT NULL,
                    NeedsPickup INTEGER NOT NULL,
                    RecurrenceRule TEXT NULL,
                    Note TEXT NULL,
                    ChildName TEXT NOT NULL,
                    RequestedByUserId INTEGER NOT NULL,
                    RequestedByEmail TEXT NOT NULL,
                    CreatedUtc TEXT NOT NULL,
                    Status TEXT NOT NULL,
                    ReviewedUtc TEXT NULL,
                    ReviewedBy TEXT NULL,
                    ReviewNote TEXT NULL
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
