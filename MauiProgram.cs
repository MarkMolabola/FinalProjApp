using FinalProjApp.Data;
using FinalProjApp.Models;
using FinalProjApp.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MudBlazor.Services;
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
            builder.Services.AddMudServices();
            builder.Services.AddBlazorWebViewDeveloperTools();
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
