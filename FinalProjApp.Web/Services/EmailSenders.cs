using Azure;
using Azure.Communication.Email;

namespace FinalProjApp.Web.Services
{
    /// <summary>Sends an email. Returns false when it couldn't (e.g. email isn't set up yet).</summary>
    public interface IEmailSender
    {
        Task<bool> SendAsync(string to, string subject, string html);
    }

    /// <summary>
    /// Used until Azure Communication Services is configured: writes the email to the
    /// log instead, and reports it as not sent so the admin is shown the link to copy.
    /// </summary>
    public class LogEmailSender : IEmailSender
    {
        private readonly ILogger<LogEmailSender> _log;
        public LogEmailSender(ILogger<LogEmailSender> log) => _log = log;

        public Task<bool> SendAsync(string to, string subject, string html)
        {
            _log.LogWarning("Email isn't set up (Email:ConnectionString). Would have sent to {To}: {Subject}\n{Body}", to, subject, html);
            return Task.FromResult(false);
        }
    }

    /// <summary>Real email through Azure Communication Services (Email:ConnectionString + Email:Sender).</summary>
    public class AcsEmailSender : IEmailSender
    {
        private readonly EmailClient _client;
        private readonly string _sender;
        private readonly ILogger<AcsEmailSender> _log;

        public AcsEmailSender(IConfiguration config, ILogger<AcsEmailSender> log)
        {
            _client = new EmailClient(config["Email:ConnectionString"]);
            _sender = config["Email:Sender"] ?? throw new InvalidOperationException("Set Email:Sender, e.g. DoNotReply@<your-domain>.");
            _log = log;
        }

        public async Task<bool> SendAsync(string to, string subject, string html)
        {
            try
            {
                await _client.SendAsync(WaitUntil.Started, _sender, to, subject, html);
                return true;
            }
            catch (RequestFailedException ex)
            {
                _log.LogError(ex, "Couldn't send email to {To}", to);
                return false;
            }
        }
    }
}
