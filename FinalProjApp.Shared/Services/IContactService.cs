using FinalProjApp.Models;

namespace FinalProjApp.Services
{
    /// <summary>
    /// Contact page messages: anyone can send one, only the admin reads them. Problems the
    /// sender can fix come back as an InvalidOperationException with a readable message.
    /// </summary>
    public interface IContactService
    {
        /// <summary>Raised after this user marks a message handled or not, so the tab count can update.</summary>
        event Action? Changed;

        Task SendAsync(ContactForm form);

        /// <summary>Messages, newest first.</summary>
        Task<List<ContactMessage>> GetAsync(AppUser admin);

        /// <summary>How many messages still need a reply. Safe to call while a page is loading its own data.</summary>
        Task<int> CountOpenAsync(AppUser admin);

        Task SetHandledAsync(AppUser admin, int messageId, bool handled);
    }
}
