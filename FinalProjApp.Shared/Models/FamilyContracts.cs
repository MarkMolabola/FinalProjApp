namespace FinalProjApp.Models
{
    /// <summary>Links a kid to one of their guardians (a kid can have several; siblings share).</summary>
    public class ChildGuardian
    {
        public int ChildId { get; set; }
        public Guid GuardianId { get; set; }
    }

    /// <summary>Whether a guardian can use the parent portal yet.</summary>
    public enum GuardianAccess
    {
        /// <summary>No email on file, so no invite can be sent.</summary>
        NoEmail,
        /// <summary>Has an email but no invite has gone out.</summary>
        NotInvited,
        /// <summary>Invite sent, account not created yet.</summary>
        Invited,
        /// <summary>Has created their account.</summary>
        Active
    }

    /// <summary>Everything the Kids page shows, in one call.</summary>
    public class FamilySnapshot
    {
        public List<Child> Kids { get; set; } = new();
        public List<Guardian> Guardians { get; set; } = new();
        public List<ChildGuardian> Links { get; set; } = new();
        public Dictionary<Guid, GuardianAccess> Access { get; set; } = new();
    }

    /// <summary>What happened when an invite was sent.</summary>
    public class InviteResult
    {
        public bool Sent { get; set; }
        public string Message { get; set; } = "";

        /// <summary>
        /// Only set while email isn't set up yet, so the admin can copy the link and
        /// pass it on by hand.
        /// </summary>
        public string? Link { get; set; }
    }

    /// <summary>The reply when a kid or guardian is added: the saved guardian and any invite sent.</summary>
    public class FamilyChange
    {
        public int ChildId { get; set; }
        public Guardian Guardian { get; set; } = new();
        public InviteResult? Invite { get; set; }
    }

    /// <summary>POST /api/family/kids body.</summary>
    public class NewKidRequest
    {
        public Child Kid { get; set; } = new();
        public Guardian Guardian { get; set; } = new();
    }

    /// <summary>What the set-your-password page shows about an invite.</summary>
    public class InviteDetails
    {
        public string Email { get; set; } = "";
        public string GuardianName { get; set; } = "";
        public List<string> KidNames { get; set; } = new();
    }

    /// <summary>POST /api/invites/{code}/accept body.</summary>
    public class AcceptInviteRequest
    {
        public string Password { get; set; } = "";
    }
}
