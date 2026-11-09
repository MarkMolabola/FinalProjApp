using FinalProjApp.Data;
using FinalProjApp.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FinalProjApp.Services
{
    /// <summary>Schedule requests in the database. Both the web pages and the API go through here.</summary>
    public class ScheduleRequestService : IScheduleRequestService
    {
        private const int MaxTextLength = 500;

        private readonly DataContext _db;
        private readonly IServiceScopeFactory _scopes;

        public ScheduleRequestService(DataContext db, IServiceScopeFactory scopes)
        {
            _db = db;
            _scopes = scopes;
        }

        public event Action? Reviewed;

        public Task<List<ScheduleRequest>> GetMineAsync(AppUser parent) =>
            _db.ScheduleRequests.AsNoTracking()
                .Where(r => r.RequestedByUserId == parent.UserId)
                .OrderByDescending(r => r.Id)
                .ToListAsync();

        public Task<List<ScheduleRequest>> GetPendingAsync(AppUser admin)
        {
            RequireAdmin(admin);
            return _db.ScheduleRequests.AsNoTracking()
                .Where(r => r.Status == ScheduleRequestStatus.Pending)
                .OrderBy(r => r.Id)
                .ToListAsync();
        }

        public async Task<int> CountPendingAsync(AppUser admin)
        {
            RequireAdmin(admin);

            // The nav bar asks while pages load, and a DbContext can only do one thing at a time
            await using var scope = _scopes.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<DataContext>();
            return await db.ScheduleRequests.CountAsync(r => r.Status == ScheduleRequestStatus.Pending);
        }

        public async Task<ScheduleRequest> SubmitAsync(AppUser parent, ScheduleRequest request)
        {
            if (parent.GuardianId is not { } guardianId)
                throw new InvalidOperationException("Only a parent account can send a schedule request.");

            var isTheirKid = await _db.ChildGuardians.AnyAsync(l => l.GuardianId == guardianId && l.ChildId == request.ChildId);
            var child = isTheirKid ? await _db.children.AsNoTracking().FirstOrDefaultAsync(c => c.ChildId == request.ChildId) : null;
            if (child == null)
                throw new InvalidOperationException("Choose one of your kids.");

            if (request.EndTime <= request.StartTime)
                throw new InvalidOperationException("The end time needs to be after the start time.");

            // Times are the parent's local time and the server may be in UTC, so allow a day either way
            if (request.StartTime < DateTime.UtcNow.AddDays(-1))
                throw new InvalidOperationException("Pick a time that hasn't passed yet.");

            if (request.RecurrenceRule != null && !ScheduleRules.IsValidRule(request.RecurrenceRule, request.StartTime))
                throw new InvalidOperationException("The repeat end date needs to be on or after the first day.");

            var note = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim();
            if (note?.Length > MaxTextLength)
                throw new InvalidOperationException("Keep the note under 500 characters.");

            var saved = new ScheduleRequest
            {
                ChildId = child.ChildId,
                ChildName = $"{child.FirstName} {child.LastName}",
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                NeedsPickup = request.NeedsPickup,
                RecurrenceRule = request.RecurrenceRule,
                Note = note,
                RequestedByUserId = parent.UserId,
                RequestedByEmail = parent.Email,
                CreatedUtc = DateTime.UtcNow
            };
            _db.ScheduleRequests.Add(saved);
            await _db.SaveChangesAsync();
            return saved;
        }

        public async Task ApproveAsync(AppUser admin, int requestId)
        {
            RequireAdmin(admin);

            var request = await _db.ScheduleRequests.AsNoTracking().FirstOrDefaultAsync(r => r.Id == requestId)
                ?? throw new InvalidOperationException("That request no longer exists.");

            if (!await _db.children.AnyAsync(c => c.ChildId == request.ChildId))
                throw new InvalidOperationException($"{request.ChildName} is no longer enrolled, so this request can only be declined.");

            var scheduleEvent = new ScheduleEvent
            {
                Subject = request.ChildName,
                ChildId = request.ChildId,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                NeedsPickup = request.NeedsPickup,
                RecurrenceRule = request.RecurrenceRule,
                Note = request.Note
            };

            // A transaction has to run inside the retry strategy, so a retry repeats all of it
            await _db.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
            {
                // Claiming the request first means two admins can't add it to the schedule twice
                await using var transaction = await _db.Database.BeginTransactionAsync();
                await ClaimAsync(admin, requestId, ScheduleRequestStatus.Approved, reviewNote: null);

                _db.scheduleEvents.Add(scheduleEvent);
                await _db.SaveChangesAsync();
                await transaction.CommitAsync();
            });
            Reviewed?.Invoke();
        }

        public async Task DeclineAsync(AppUser admin, int requestId, string? reason)
        {
            RequireAdmin(admin);

            var note = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim();
            if (note?.Length > MaxTextLength)
                throw new InvalidOperationException("Keep the reason under 500 characters.");

            await ClaimAsync(admin, requestId, ScheduleRequestStatus.Declined, note);
            Reviewed?.Invoke();
        }

        private async Task ClaimAsync(AppUser admin, int requestId, string status, string? reviewNote)
        {
            DateTime? now = DateTime.UtcNow;
            var claimed = await _db.ScheduleRequests
                .Where(r => r.Id == requestId && r.Status == ScheduleRequestStatus.Pending)
                .ExecuteUpdateAsync(set => set
                    .SetProperty(r => r.Status, status)
                    .SetProperty(r => r.ReviewedUtc, now)
                    .SetProperty(r => r.ReviewedBy, admin.Email)
                    .SetProperty(r => r.ReviewNote, reviewNote));

            if (claimed == 0)
                throw new InvalidOperationException("That request has already been reviewed.");
        }

        private static void RequireAdmin(AppUser user)
        {
            if (user.Role != "Admin")
                throw new InvalidOperationException("Only an admin can review schedule requests.");
        }
    }
}
