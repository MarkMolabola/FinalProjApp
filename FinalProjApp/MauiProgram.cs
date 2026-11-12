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

            SyncfusionLicense.Register();
            builder.Services.AddSyncfusionBlazor();

            // All data lives in the web app; the phone reads and writes it through the API
            builder.Services.AddSingleton<ApiClient>();
            builder.Services.AddScoped<ICRUD<Child>, ApiChildService>();
            builder.Services.AddScoped<ICRUD<Guardian>, ApiGuardianService>();
            builder.Services.AddScoped<ICRUD<ScheduleEvent>, ApiScheduleEventService>();
            builder.Services.AddScoped<IPhotoService, ApiPhotoService>();
            builder.Services.AddScoped<IFamilyService, ApiFamilyService>();
            builder.Services.AddScoped<IInviteService, ApiInviteService>();
            builder.Services.AddScoped<IScheduleRequestService, ApiScheduleRequestService>();
            builder.Services.AddScoped<IContactService, ApiContactService>();

            builder.Services.AddAuthorizationCore();
            builder.Services.AddScoped<CustomAuthStateProvider, PhoneAuthStateProvider>();
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
