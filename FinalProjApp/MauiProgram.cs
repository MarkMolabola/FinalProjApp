using FinalProjApp.Models;
using FinalProjApp.Services;
using FinalProjApp.Services.Api;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;
using Syncfusion.Blazor;

namespace FinalProjApp
{
    public static class MauiProgram
    {
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

            // All data lives in the web app; the phone reads and writes it through the API
            builder.Services.AddSingleton<ApiClient>();
            builder.Services.AddScoped<ICRUD<Child>, ApiChildService>();
            builder.Services.AddScoped<ICRUD<Guardian>, ApiGuardianService>();
            builder.Services.AddScoped<ICRUD<ScheduleEvent>, ApiScheduleEventService>();
            builder.Services.AddScoped<IPhotoService, ApiPhotoService>();

            builder.Services.AddAuthorizationCore();
            builder.Services.AddScoped<CustomAuthStateProvider>();
            builder.Services.AddScoped<AuthenticationStateProvider>(sp =>
                sp.GetRequiredService<CustomAuthStateProvider>());
            builder.Services.AddScoped<IAuthService, MauiAuthService>();

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
