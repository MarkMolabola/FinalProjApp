using FinalProjApp.Data;
using FinalProjApp.Models;
using FinalProjApp.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Syncfusion.Blazor;

namespace FinalProjApp
{
    public static class MauiProgram
    {
        // For now the phone keeps its own database on the device; it moves to the web API next
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

            builder.Services.AddDbContext<DataContext>(options => options.UseSqlite(DbPath));
            builder.Services.AddScoped<ICRUD<Child>, ChildCRUD>();
            builder.Services.AddScoped<ICRUD<Guardian>, GuardianCRUD>();
            builder.Services.AddScoped<ICRUD<ScheduleEvent>, ScheduleEventCRUD>();
            builder.Services.AddScoped<IPhotoService, PhotoService>();

            builder.Services.AddAuthorizationCore();
            builder.Services.AddScoped<CustomAuthStateProvider>();
            builder.Services.AddScoped<AuthenticationStateProvider>(sp =>
                sp.GetRequiredService<CustomAuthStateProvider>());
            builder.Services.AddScoped<UserAccounts>();
            builder.Services.AddScoped<IAuthService, MauiAuthService>();

            using (var scope = builder.Services.BuildServiceProvider().CreateScope())
                DbInitializer.Initialize(scope.ServiceProvider.GetRequiredService<DataContext>());

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
