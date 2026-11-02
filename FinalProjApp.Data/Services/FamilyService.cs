using FinalProjApp.Data;
using FinalProjApp.Models;
using Microsoft.EntityFrameworkCore;

namespace FinalProjApp.Services
{
    /// <summary>
    /// Kids and guardians in the database. Guardians are matched by email, so adding
    /// a sibling with a known parent's email links that same parent (one account
    /// then sees both kids).
    /// </summary>
    public class FamilyService : IFamilyService
    {
        private readonly DataContext _db;
        private readonly IInviteService _invites;

        public FamilyService(DataContext db, IInviteService invites)
        {
            _db = db;
            _invites = invites;
        }

        public async Task<FamilySnapshot> GetAllAsync()
        {
            var guardians = await _db.guardian.AsNoTracking().ToListAsync();
            var withAccount = (await _db.Users.Where(u => u.GuardianId != null).Select(u => u.GuardianId!.Value).ToListAsync())
                .ToHashSet();
            var now = DateTime.UtcNow;
            var invited = (await _db.Invites.Where(i => i.UsedUtc == null && i.ExpiresUtc > now)
                    .Select(i => i.GuardianId).ToListAsync())
                .ToHashSet();

            return new FamilySnapshot
            {
                Kids = await _db.children.AsNoTracking().ToListAsync(),
                Guardians = guardians,
                Links = await _db.ChildGuardians.AsNoTracking().ToListAsync(),
                Access = guardians.ToDictionary(g => g.GuardianId, g =>
                    withAccount.Contains(g.GuardianId) ? GuardianAccess.Active
                    : invited.Contains(g.GuardianId) ? GuardianAccess.Invited
                    : string.IsNullOrEmpty(g.Email) ? GuardianAccess.NoEmail
                    : GuardianAccess.NotInvited)
            };
        }

        public Task<List<Child>> GetKidsOfGuardianAsync(Guid guardianId) =>
            _db.children.AsNoTracking()
                .Where(c => _db.ChildGuardians.Any(l => l.GuardianId == guardianId && l.ChildId == c.ChildId))
                .ToListAsync();

        public async Task<FamilyChange> AddKidAsync(Child kid, Guardian guardian)
        {
            var (saved, isNew) = await FindOrAddGuardianAsync(guardian);

            kid.ChildId = 0; // the database picks the id
            kid.GuardianID = saved.GuardianId;
            _db.children.Add(kid);
            await _db.SaveChangesAsync();

            _db.ChildGuardians.Add(new ChildGuardian { ChildId = kid.ChildId, GuardianId = saved.GuardianId });
            await _db.SaveChangesAsync();

            return new FamilyChange { ChildId = kid.ChildId, Guardian = saved, Invite = await InviteIfNewAsync(saved, isNew) };
        }

        public async Task UpdateKidAsync(Child changes)
        {
            var kid = await _db.children.FindAsync(changes.ChildId) ?? throw new KeyNotFoundException("That kid wasn't found.");
            kid.FirstName = changes.FirstName;
            kid.LastName = changes.LastName;
            kid.DateofBirth = changes.DateofBirth;
            await _db.SaveChangesAsync();
        }

        public async Task DeleteKidAsync(int childId)
        {
            var kid = await _db.children.FindAsync(childId);
            if (kid == null) return;

            var links = await _db.ChildGuardians.Where(l => l.ChildId == childId).ToListAsync();
            _db.ChildGuardians.RemoveRange(links);
            _db.children.Remove(kid);
            await _db.SaveChangesAsync();

            foreach (var link in links)
                await RemoveIfOrphanedAsync(link.GuardianId);
        }

        public async Task<FamilyChange> AddGuardianAsync(int childId, Guardian guardian)
        {
            if (await _db.children.FindAsync(childId) == null) throw new KeyNotFoundException("That kid wasn't found.");

            var (saved, isNew) = await FindOrAddGuardianAsync(guardian);
            if (!await _db.ChildGuardians.AnyAsync(l => l.ChildId == childId && l.GuardianId == saved.GuardianId))
            {
                _db.ChildGuardians.Add(new ChildGuardian { ChildId = childId, GuardianId = saved.GuardianId });
                await _db.SaveChangesAsync();
            }

            return new FamilyChange { ChildId = childId, Guardian = saved, Invite = await InviteIfNewAsync(saved, isNew) };
        }

        public async Task UpdateGuardianAsync(Guardian changes)
        {
            var guardian = await _db.guardian.FindAsync(changes.GuardianId) ?? throw new KeyNotFoundException("That guardian wasn't found.");
            var email = NormalizeEmail(changes.Email);

            if (email != null && email != guardian.Email
                && await _db.guardian.AnyAsync(g => g.Email == email && g.GuardianId != guardian.GuardianId))
                throw new InvalidOperationException("Another guardian already uses that email.");

            // A new email means any invite sent to the old one shouldn't work any more
            if (email != guardian.Email)
                await VoidInvitesAsync(guardian.GuardianId);

            guardian.FullName = changes.FullName;
            guardian.PhoneNumber = changes.PhoneNumber;
            guardian.Address = changes.Address;
            guardian.Email = email;
            await _db.SaveChangesAsync();
        }

        public async Task RemoveGuardianAsync(int childId, Guid guardianId)
        {
            var links = await _db.ChildGuardians.Where(l => l.ChildId == childId).ToListAsync();
            var link = links.FirstOrDefault(l => l.GuardianId == guardianId);
            if (link == null) return;
            if (links.Count == 1) throw new InvalidOperationException("A kid needs at least one guardian.");

            _db.ChildGuardians.Remove(link);

            // Keep the kid's main guardian pointing at someone who's still linked
            var kid = await _db.children.FindAsync(childId);
            if (kid != null && kid.GuardianID == guardianId)
                kid.GuardianID = links.First(l => l.GuardianId != guardianId).GuardianId;

            await _db.SaveChangesAsync();
            await RemoveIfOrphanedAsync(guardianId);
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        /// <summary>The guardian with this email if there is one (siblings), otherwise a new one.</summary>
        private async Task<(Guardian Guardian, bool IsNew)> FindOrAddGuardianAsync(Guardian guardian)
        {
            var email = NormalizeEmail(guardian.Email);
            if (email != null && await _db.guardian.FirstOrDefaultAsync(g => g.Email == email) is { } existing)
                return (existing, false);

            guardian.GuardianId = Guid.NewGuid();
            guardian.Email = email;
            _db.guardian.Add(guardian);
            await _db.SaveChangesAsync();
            return (guardian, true);
        }

        private async Task<InviteResult?> InviteIfNewAsync(Guardian guardian, bool isNew) =>
            isNew && guardian.Email != null ? await _invites.SendAsync(guardian.GuardianId) : null;

        /// <summary>A guardian with no kids left and no account isn't needed any more.</summary>
        private async Task RemoveIfOrphanedAsync(Guid guardianId)
        {
            if (await _db.ChildGuardians.AnyAsync(l => l.GuardianId == guardianId)) return;
            if (await _db.Users.AnyAsync(u => u.GuardianId == guardianId)) return;

            var guardian = await _db.guardian.FindAsync(guardianId);
            if (guardian == null) return;
            _db.Invites.RemoveRange(_db.Invites.Where(i => i.GuardianId == guardianId));
            _db.guardian.Remove(guardian);
            await _db.SaveChangesAsync();
        }

        private async Task VoidInvitesAsync(Guid guardianId)
        {
            var now = DateTime.UtcNow;
            foreach (var invite in await _db.Invites.Where(i => i.GuardianId == guardianId && i.UsedUtc == null).ToListAsync())
                invite.UsedUtc = now;
        }

        private static string? NormalizeEmail(string? email) =>
            string.IsNullOrWhiteSpace(email) ? null : email.Trim().ToLowerInvariant();
    }
}
