using FinalProjApp.Models;

namespace FinalProjApp.Services
{
    /// <summary>
    /// Parents ask for a schedule; the admin confirms or declines it. Problems the user
    /// can fix come back as an InvalidOperationException with a readable message.
    /// </summary>
    public interface IScheduleRequestService
    {
        /// <summary>Raised after this user confirms or declines a request, so the tab count can update.</summary>
        event Action? Reviewed;

        /// <summary>The requests this parent has sent, newest first.</summary>
        Task<List<ScheduleRequest>> GetMineAsync(AppUser parent);

        /// <summary>Requests waiting for review, oldest first.</summary>
        Task<List<ScheduleRequest>> GetPendingAsync(AppUser admin);

        /// <summary>How many requests are waiting. Safe to call while a page is loading its own data.</summary>
        Task<int> CountPendingAsync(AppUser admin);

        Task<ScheduleRequest> SubmitAsync(AppUser parent, ScheduleRequest request);

        /// <summary>Confirms the request and adds it to the schedule.</summary>
        Task ApproveAsync(AppUser admin, int requestId);

        Task DeclineAsync(AppUser admin, int requestId, string? reason);
    }
}
