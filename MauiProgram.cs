using FinalProjApp.Data;
using FinalProjApp.Models;
using FinalProjApp.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MudBlazor.Services;
using Syncfusion.Blazor;
using Syncfusion.Blazor.Schedule;
using static System.Formats.Asn1.AsnWriter;

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
            builder.Services.AddMudServices();
            builder.Services.AddMauiBlazorWebView();

#if DEBUG
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Ngo9BigBOggjHTQxAR8/V1JFaF5cXGRCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdmWH9ccHVSR2ZcVEd/WktWYEg=");
           // builder.Services.AddMudServices();
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Services.AddSyncfusionBlazor();
            builder.Services.AddScoped<ICRUD<Child>, ChildCRUD>();
            builder.Services.AddScoped<ICRUD<Guardian>, GuardianCRUD>();
            builder.Services.AddScoped<ICRUD<ScheduleEvent>, ScheduleEventCRUD>();

            builder.Services.AddDbContext<Data.DataContext>(options =>
            {
                options.UseSqlite("Data Source=C:\\DATA\\DaycareSystem.db");
             
            });
            using (var scope = builder.Services.BuildServiceProvider().CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<DataContext>();
                
                dbContext.Database.EnsureCreated();
            }

            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
