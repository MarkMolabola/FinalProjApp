using Microsoft.Extensions.Caching.Memory;

namespace FinalProjApp.Web.Services
{
    /// <summary>
    /// Stops password guessing: each email gets a limited number of tries, shared by the
    /// web sign-in page and the phone app's API. A successful sign-in resets the count.
    /// </summary>
    public class SignInThrottle
    {
        private const int MaxAttempts = 5;
        private static readonly TimeSpan Window = TimeSpan.FromMinutes(15);

        private readonly IMemoryCache _attempts;
        private readonly object _lock = new();

        public SignInThrottle(IMemoryCache attempts) => _attempts = attempts;

        /// <summary>
        /// Counts a try and says whether it may go ahead. Counting before the password is
        /// checked means tries sent at the same moment can't slip past the limit.
        /// </summary>
        public bool TryStart(string email)
        {
            lock (_lock)
            {
                var counter = _attempts.GetOrCreate(Key(email), entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = Window;
                    return new Counter();
                })!;
                return ++counter.Attempts <= MaxAttempts;
            }
        }

        public void Reset(string email)
        {
            lock (_lock) _attempts.Remove(Key(email));
        }

        private static string Key(string email) => $"signin-attempts:{email.Trim().ToLowerInvariant()}";

        private sealed class Counter
        {
            public int Attempts;
        }
    }
}
