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
            MapFamily(api.MapGroup("/family").RequireAuthorization(StaffOnly));
            MapInvites(api.MapGroup("/invites"));
        }

        // ── Kids page: kids + guardians + invites (staff only) ───────────────

        private static void MapFamily(RouteGroupBuilder family)
        {
            family.MapGet("/", (IFamilyService f) => f.GetAllAsync());

            family.MapPost("/kids", async (NewKidRequest body, IFamilyService f) =>
                Results.Ok(await f.AddKidAsync(body.Kid, body.Guardian)));

            family.MapPut("/kids/{id:int}", async (int id, Child kid, IFamilyService f) =>
            {
                kid.ChildId = id;
                await f.UpdateKidAsync(kid);
                return Results.NoContent();
            });

            family.MapDelete("/kids/{id:int}", async (int id, IFamilyService f) =>
            {
                await f.DeleteKidAsync(id);
                return Results.NoContent();
            });

            family.MapPost("/kids/{id:int}/guardians", async (int id, Guardian guardian, IFamilyService f) =>
                Results.Ok(await f.AddGuardianAsync(id, guardian)));

            family.MapDelete("/kids/{id:int}/guardians/{guardianId:guid}", async (int id, Guid guardianId, IFamilyService f) =>
            {
                try
                {
                    await f.RemoveGuardianAsync(id, guardianId);
                    return Results.NoContent();
                }
                catch (InvalidOperationException ex)
                {
                    return Results.Conflict(ex.Message);
                }
            });

            family.MapPut("/guardians/{id:guid}", async (Guid id, Guardian guardian, IFamilyService f) =>
            {
                guardian.GuardianId = id;
                try
                {
                    await f.UpdateGuardianAsync(guardian);
                    return Results.NoContent();
                }
                catch (InvalidOperationException ex)
                {
                    return Results.Conflict(ex.Message);
                }
            });

            family.MapPost("/guardians/{id:guid}/invite", (Guid id, IInviteService invites) => invites.SendAsync(id));
        }

        // ── Invite links (no sign-in: the code in the link is the key) ───────

        private static void MapInvites(RouteGroupBuilder invites)
        {
            invites.MapGet("/{code}", async (string code, IInviteService service) =>
                await service.GetAsync(code) is { } details ? Results.Ok(details) : Results.NotFound());

            invites.MapPost("/{code}/accept", async (string code, AcceptInviteRequest body, IInviteService service) =>
                await service.AcceptAsync(code, body.Password) is { } error
                    ? Results.BadRequest(error)
                    : Results.NoContent());
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

        // ── Photos: public Showcase for guests, Families for signed-in users ─

        private static void MapPhotos(RouteGroupBuilder photos)
        {
            // Anyone, signed in or not, can see the Showcase the admin picked
            photos.MapGet("/showcase", (IPhotoService service) => service.GetAsync(publicAlbum: true));

            photos.MapGet("/families", (IPhotoService service) => service.GetAsync(publicAlbum: false))
                .RequireAuthorization(SignedIn);

            photos.MapPost("/", async (Photo photo, IPhotoService service) =>
            {
                if (photo.Data.Length == 0 || photo.Data.Length > MaxPhotoBytes)
                    return Results.BadRequest("Photos must be between 1 byte and 15 MB.");
                if (!photo.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                    return Results.BadRequest("Only images can be uploaded.");

                var saved = await service.AddAsync(photo.FileName, photo.ContentType, photo.Data, photo.IsPublic);
                return Results.Created($"/api/photos/{saved.Id}", saved);
            }).RequireAuthorization(AdminOnly);

            photos.MapPut("/{id:int}/album", async (int id, bool isPublic, IPhotoService service) =>
            {
                await service.MoveAsync(id, isPublic);
                return Results.NoContent();
            }).RequireAuthorization(AdminOnly);

            photos.MapDelete("/{id:int}", async (int id, IPhotoService service) =>
            {
                await service.DeleteAsync(id);
                return Results.NoContent();
            }).RequireAuthorization(AdminOnly);
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private static bool IsStaff(ClaimsPrincipal user) => user.IsInRole("Admin") || user.IsInRole("Staff");

        /// <summary>Staff see every kid; a parent sees only the kids linked to them.</summary>
        private static IQueryable<Child> VisibleChildren(ClaimsPrincipal user, DataContext db)
        {
            if (IsStaff(user)) return db.children;
            var guardianId = user.ToAppUser()?.GuardianId;
            return guardianId is { } id
                ? db.children.Where(c => db.ChildGuardians.Any(l => l.GuardianId == id && l.ChildId == c.ChildId))
                : db.children.Where(_ => false);
        }
    }
}
