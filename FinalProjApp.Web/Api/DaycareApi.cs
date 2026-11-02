using FinalProjApp.Data;
using FinalProjApp.Models;
using FinalProjApp.Services;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FinalProjApp.Web.Api
{
    /// <summary>
    /// The REST API the phone app uses. Every call needs a token from /api/auth/login.
    /// Staff and admins see everything; a parent only ever gets their own kids and
    /// those kids' events (filtered here, on the server).
    /// </summary>
    public static class DaycareApi
    {
        public const string SignedIn = "api-signed-in";
        public const string StaffOnly = "api-staff";
        public const string AdminOnly = "api-admin";

        /// <summary>Phone cameras take big pictures; matches the Photos page limit.</summary>
        public const long MaxPhotoBytes = 15 * 1024 * 1024;

        public static void MapDaycareApi(this WebApplication app)
        {
            var api = app.MapGroup("/api");

            api.MapPost("/auth/login", async (LoginRequest body, UserAccounts accounts, TokenService tokens) =>
                await accounts.CheckPasswordAsync(body.Email ?? "", body.Password ?? "") is { } user
                    ? Results.Ok(tokens.Issue(user))
                    : Results.Unauthorized());

            MapChildren(api.MapGroup("/children"));
            MapGuardians(api.MapGroup("/guardians").RequireAuthorization(StaffOnly));
            MapEvents(api.MapGroup("/events"));
            MapPhotos(api.MapGroup("/photos"));
        }

        // ── Kids ─────────────────────────────────────────────────────────────

        private static void MapChildren(RouteGroupBuilder kids)
        {
            kids.MapGet("/", async (ClaimsPrincipal user, DataContext db) =>
                await VisibleChildren(user, db).AsNoTracking().ToListAsync())
                .RequireAuthorization(SignedIn);

            kids.MapPost("/", async (Child child, DataContext db) =>
            {
                child.ChildId = 0; // the database picks the id
                db.children.Add(child);
                await db.SaveChangesAsync();
                return Results.Created($"/api/children/{child.ChildId}", child);
            }).RequireAuthorization(StaffOnly);

            kids.MapPut("/{id:int}", async (int id, Child changes, DataContext db) =>
            {
                var child = await db.children.FindAsync(id);
                if (child == null) return Results.NotFound();
                child.FirstName = changes.FirstName;
                child.LastName = changes.LastName;
                child.DateofBirth = changes.DateofBirth;
                child.GuardianID = changes.GuardianID;
                await db.SaveChangesAsync();
                return Results.NoContent();
            }).RequireAuthorization(StaffOnly);

            kids.MapDelete("/{id:int}", async (int id, DataContext db) =>
            {
                var child = await db.children.FindAsync(id);
                if (child == null) return Results.NotFound();
                db.children.Remove(child);
                await db.SaveChangesAsync();
                return Results.NoContent();
            }).RequireAuthorization(StaffOnly);
        }

        // ── Guardians (staff only) ───────────────────────────────────────────

        private static void MapGuardians(RouteGroupBuilder guardians)
        {
            guardians.MapGet("/", async (DataContext db) =>
                await db.guardian.AsNoTracking().ToListAsync());

            guardians.MapGet("/{id:guid}", async (Guid id, DataContext db) =>
                await db.guardian.FindAsync(id) is { } g ? Results.Ok(g) : Results.NotFound());

            guardians.MapPost("/", async (Guardian guardian, DataContext db) =>
            {
                if (guardian.GuardianId == Guid.Empty) guardian.GuardianId = Guid.NewGuid();
                db.guardian.Add(guardian);
                await db.SaveChangesAsync();
                return Results.Created($"/api/guardians/{guardian.GuardianId}", guardian);
            });

            guardians.MapPut("/{id:guid}", async (Guid id, Guardian changes, DataContext db) =>
            {
                var guardian = await db.guardian.FindAsync(id);
                if (guardian == null) return Results.NotFound();
                guardian.FullName = changes.FullName;
                guardian.PhoneNumber = changes.PhoneNumber;
                guardian.Address = changes.Address;
                await db.SaveChangesAsync();
                return Results.NoContent();
            });

            guardians.MapDelete("/{id:guid}", async (Guid id, DataContext db) =>
            {
                var guardian = await db.guardian.FindAsync(id);
                if (guardian == null) return Results.NotFound();
                db.guardian.Remove(guardian);
                await db.SaveChangesAsync();
                return Results.NoContent();
            });
        }

        // ── Schedule events ──────────────────────────────────────────────────

        private static void MapEvents(RouteGroupBuilder events)
        {
            events.MapGet("/", async (ClaimsPrincipal user, DataContext db) =>
            {
                var all = db.scheduleEvents.AsNoTracking();
                if (IsStaff(user)) return await all.ToListAsync();

                var kidIds = VisibleChildren(user, db).Select(c => c.ChildId);
                return await all.Where(e => kidIds.Contains(e.ChildId)).ToListAsync();
            }).RequireAuthorization(SignedIn);

            events.MapPost("/", async (ScheduleEvent ev, DataContext db) =>
            {
                ev.Id = 0;
                db.scheduleEvents.Add(ev);
                await db.SaveChangesAsync();
                return Results.Created($"/api/events/{ev.Id}", ev);
            }).RequireAuthorization(StaffOnly);

            events.MapPut("/{id:int}", async (int id, ScheduleEvent changes, DataContext db) =>
            {
                var ev = await db.scheduleEvents.FindAsync(id);
                if (ev == null) return Results.NotFound();
                changes.Id = id;
                db.Entry(ev).CurrentValues.SetValues(changes);
                await db.SaveChangesAsync();
                return Results.NoContent();
            }).RequireAuthorization(StaffOnly);

            events.MapDelete("/{id:int}", async (int id, DataContext db) =>
            {
                var ev = await db.scheduleEvents.FindAsync(id);
                if (ev == null) return Results.NotFound();
                db.scheduleEvents.Remove(ev);
                await db.SaveChangesAsync();
                return Results.NoContent();
            }).RequireAuthorization(StaffOnly);
        }

        // ── Photos (anyone signed in can look; only admins add) ──────────────

        private static void MapPhotos(RouteGroupBuilder photos)
        {
            photos.MapGet("/", async (IPhotoService service) => await service.GetAllAsync())
                .RequireAuthorization(SignedIn);

            photos.MapPost("/", async (Photo photo, IPhotoService service) =>
            {
                if (photo.Data.Length == 0 || photo.Data.Length > MaxPhotoBytes)
                    return Results.BadRequest("Photos must be between 1 byte and 15 MB.");
                if (!photo.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                    return Results.BadRequest("Only images can be uploaded.");

                var saved = await service.AddAsync(photo.FileName, photo.ContentType, photo.Data);
                return Results.Created($"/api/photos/{saved.Id}", saved);
            }).RequireAuthorization(AdminOnly);
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private static bool IsStaff(ClaimsPrincipal user) => user.IsInRole("Admin") || user.IsInRole("Staff");

        /// <summary>Staff see every kid; a parent sees only the kids linked to their guardian record.</summary>
        private static IQueryable<Child> VisibleChildren(ClaimsPrincipal user, DataContext db)
        {
            if (IsStaff(user)) return db.children;
            var guardianId = user.ToAppUser()?.GuardianId;
            return guardianId is { } id ? db.children.Where(c => c.GuardianID == id) : db.children.Where(_ => false);
        }
    }
}
