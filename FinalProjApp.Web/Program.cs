using FinalProjApp.Data;
using FinalProjApp.Models;
using FinalProjApp.Services;
using FinalProjApp.Web.Api;
using FinalProjApp.Web.Components;
using FinalProjApp.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Claims;
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
builder.Services.AddScoped<IFamilyService, FamilyService>();
builder.Services.AddScoped<IInviteService, InviteService>();
builder.Services.AddHttpContextAccessor();

// Invite emails go through Azure Communication Services once it's configured;
// until then they're logged and the admin gets the link to copy
if (builder.Configuration["Email:ConnectionString"] is { Length: > 0 })
    builder.Services.AddSingleton<IEmailSender, AcsEmailSender>();
else
    builder.Services.AddSingleton<IEmailSender, LogEmailSender>();

// Sign-in: a cookie, so refreshing the page keeps you signed in
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.AccessDeniedPath = "/";
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.SlidingExpiration = true;
    })
    // The phone app's API calls carry a token instead (see Api/TokenService.cs)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new()
        {
            ValidIssuer = TokenService.Issuer,
            ValidAudience = TokenService.Audience,
            IssuerSigningKey = TokenService.SigningKey(builder.Configuration),
            NameClaimType = ClaimTypes.Name,
            RoleClaimType = ClaimTypes.Role
        };
    });
builder.Services.AddAuthorization(options =>
{
    // API endpoints accept tokens only, never the browser cookie
    var bearer = JwtBearerDefaults.AuthenticationScheme;
    options.AddPolicy(DaycareApi.SignedIn, p => p.AddAuthenticationSchemes(bearer).RequireAuthenticatedUser());
    options.AddPolicy(DaycareApi.StaffOnly, p => p.AddAuthenticationSchemes(bearer).RequireRole("Admin", "Staff"));
    options.AddPolicy(DaycareApi.AdminOnly, p => p.AddAuthenticationSchemes(bearer).RequireRole("Admin"));
});
builder.Services.AddSingleton<TokenService>();
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

app.MapDaycareApi();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddAdditionalAssemblies(typeof(FinalProjApp.Components.Routes).Assembly);

app.Run();
