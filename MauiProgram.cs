using FinalProjApp.Data;
using FinalProjApp.Models;
using FinalProjApp.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MudBlazor.Services;
using Syncfusion.Blazor;
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
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Ngo9BigBOggjHTQxAR8/V1JFaF5cXGRCf1NpRGZGfV5ycUVDallXTnddUj0eQnxTdEBiWX9WcXNQQGFeVE13VkleYg==");
           // builder.Services.AddMudServices();
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Services.AddSyncfusionBlazor();
            builder.Services.AddScoped<ICRUD<Child>, ChildCRUD>();
            builder.Services.AddScoped<ICRUD<Guardian>, GuardianCRUD>();
          
            builder.Services.AddDbContext<Data.DataContext>(options =>
            {
                options.UseSqlite("Data Source=DaycareSystem.db");
             
            });
       
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
