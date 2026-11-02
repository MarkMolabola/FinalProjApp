using FinalProjApp.Models;

namespace FinalProjApp.Services
{
    /// <summary>Kids, their guardians, and the links between them (the Kids page).</summary>
    public interface IFamilyService
    {
        Task<FamilySnapshot> GetAllAsync();

        /// <summary>A guardian's kids (the parent portal).</summary>
        Task<List<Child>> GetKidsOfGuardianAsync(Guid guardianId);

        /// <summary>
        /// Adds a kid with their first guardian. A guardian whose email is already on
        /// file is reused (siblings); a new guardian with an email is invited.
        /// </summary>
        Task<FamilyChange> AddKidAsync(Child kid, Guardian guardian);

        Task UpdateKidAsync(Child kid);

        /// <summary>Removes the kid, and any guardian left with no kids and no account.</summary>
        Task DeleteKidAsync(int childId);

        /// <summary>Adds (or links, by email) another guardian to a kid, inviting them if new.</summary>
        Task<FamilyChange> AddGuardianAsync(int childId, Guardian guardian);

        Task UpdateGuardianAsync(Guardian guardian);

        /// <summary>Unlinks a guardian from a kid. A kid always keeps at least one.</summary>
        Task RemoveGuardianAsync(int childId, Guid guardianId);
    }

    /// <summary>Parent-portal invites: sending them and turning them into accounts.</summary>
    public interface IInviteService
    {
        /// <summary>(Re)sends the invite; any earlier unused invite for this guardian stops working.</summary>
        Task<InviteResult> SendAsync(Guid guardianId);

        /// <summary>The invite behind a link, or null if it's unknown, used or expired.</summary>
        Task<InviteDetails?> GetAsync(string code);

        /// <summary>Creates the parent's account. Returns null on success, otherwise a message.</summary>
        Task<string?> AcceptAsync(string code, string password);
    }
}
