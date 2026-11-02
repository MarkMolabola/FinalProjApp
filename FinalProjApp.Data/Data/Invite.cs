namespace FinalProjApp.Data
{
    /// <summary>
    /// A parent-portal invite. Only a hash of the link's code is stored, so a copy of
    /// the database can't be used to claim someone's invite.
    /// </summary>
    public class Invite
    {
        public int Id { get; set; }
        public Guid GuardianId { get; set; }
        public string Email { get; set; } = "";
        public string CodeHash { get; set; } = "";
        public DateTime CreatedUtc { get; set; }
        public DateTime ExpiresUtc { get; set; }

        /// <summary>When the parent created their account (or the invite was replaced).</summary>
        public DateTime? UsedUtc { get; set; }
    }
}
