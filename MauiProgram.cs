using FinalProjApp.Data;
using FinalProjApp.Models;
using FinalProjApp.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MudBlazor.Services;
using Syncfusion.Blazor;
using Syncfusion.Blazor.Schedule;
using XCalendar.Maui;

namespace FinalProjApp
{
    public static class MauiProgram
    {
        private static string DbPath =>
            $"Data Source={Path.Combine(FileSystem.AppDataDirectory, "DaycareSystem.db")}";

        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder.Services.AddMauiBlazorWebView();

            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Ngo9BigBOggjHTQxAR8/V1JFaF5cXGRCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdmWH9ccHVSR2ZcVEd/WktWYEg=");
            builder.Services.AddSyncfusionBlazor();

            builder.Services.AddScoped<ICRUD<Child>, ChildCRUD>();
            builder.Services.AddScoped<ICRUD<Guardian>, GuardianCRUD>();
            builder.Services.AddScoped<ICRUD<ScheduleEvent>, ScheduleEventCRUD>();

            builder.Services.AddAuthorizationCore();
            builder.Services.AddScoped<CustomAuthStateProvider>();
            builder.Services.AddScoped<AuthenticationStateProvider>(sp =>
                sp.GetRequiredService<CustomAuthStateProvider>());
            builder.Services.AddScoped<AuthService>();

            builder.Services.AddDbContext<DataContext>(options =>
                options.UseSqlite(DbPath));

            using (var scope = builder.Services.BuildServiceProvider().CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<DataContext>();
                db.Database.EnsureCreated();

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
                    var salt = AuthService.GenerateSalt();
                    db.Users.Add(new AppUser
                    {
                        Email = "admin@calybear.com",
                        Salt = salt,
                        PasswordHash = AuthService.HashPassword("Admin123!", salt),
                        Role = "Admin"
                    });
                    db.SaveChanges();
                }
            }

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
