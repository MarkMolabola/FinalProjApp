using FinalProjApp.Data;
using FinalProjApp.Models;
using FinalProjApp.Services;
using FinalProjApp.Web.Components;
using FinalProjApp.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Syncfusion.Blazor;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Syncfusion key comes from configuration (user secrets locally, an app setting on Azure)
if (builder.Configuration["Syncfusion:LicenseKey"] is { Length: > 0 } syncfusionKey)
    Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(syncfusionKey);
builder.Services.AddSyncfusionBlazor();

// Database: a SQLite file, App_Data/DaycareSystem.db unless configured otherwise
var connectionString = builder.Configuration.GetConnectionString("Daycare")
    ?? $"Data Source={Path.Combine(builder.Environment.ContentRootPath, "App_Data", "DaycareSystem.db")}";
builder.Services.AddDbContext<DataContext>(options => options.UseSqlite(connectionString));
builder.Services.AddScoped<ICRUD<Child>, ChildCRUD>();
builder.Services.AddScoped<ICRUD<Guardian>, GuardianCRUD>();
builder.Services.AddScoped<ICRUD<ScheduleEvent>, ScheduleEventCRUD>();
builder.Services.AddScoped<IPhotoService, PhotoService>();

// Sign-in: a cookie, so refreshing the page keeps you signed in
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.AccessDeniedPath = "/";
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.SlidingExpiration = true;
    });
builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddMemoryCache();
builder.Services.AddScoped<UserAccounts>();
builder.Services.AddScoped<IAuthService, WebAuthService>();

var app = builder.Build();

// Create the database (and the admin account) on first run
Directory.CreateDirectory(Path.Combine(app.Environment.ContentRootPath, "App_Data"));
using (var scope = app.Services.CreateScope())
    DbInitializer.Initialize(scope.ServiceProvider.GetRequiredService<DataContext>());

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapStaticAssets();

// Completes a sign-in started by WebAuthService: trade the one-time ticket for a cookie
app.MapGet("/account/signin", async (HttpContext http, IMemoryCache tickets, string? ticket) =>
{
    var user = WebAuthService.RedeemTicket(tickets, ticket);
    if (user == null) return Results.Redirect("/login");

    var scheme = CookieAuthenticationDefaults.AuthenticationScheme;
    await http.SignInAsync(scheme, AuthClaims.CreatePrincipal(user, scheme));
    return Results.Redirect(AuthClaims.LandingPage(user.Role));
});

app.MapGet("/account/signout", async (HttpContext http) =>
{
    await http.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Redirect("/login");
});

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddAdditionalAssemblies(typeof(FinalProjApp.Components.Routes).Assembly);

app.Run();
