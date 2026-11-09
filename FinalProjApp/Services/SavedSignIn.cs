using FinalProjApp.Models;
using System.Text.Json;

namespace FinalProjApp.Services
{
    /// <summary>
    /// Remembers the phone's sign-in between app launches, in the device's secure storage.
    /// If that storage can't be used, the person simply signs in again.
    /// </summary>
    public static class SavedSignIn
    {
        private const string StorageKey = "calybear.signin";

        public static async Task SaveAsync(LoginResponse login)
        {
            try
            {
                await SecureStorage.Default.SetAsync(StorageKey, JsonSerializer.Serialize(login));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Couldn't save sign-in: {ex.Message}");
            }
        }

        /// <summary>The saved sign-in, or null if there isn't one or its token has expired.</summary>
        public static async Task<LoginResponse?> LoadAsync()
        {
            try
            {
                var json = await SecureStorage.Default.GetAsync(StorageKey);
                if (json == null) return null;

                var login = JsonSerializer.Deserialize<LoginResponse>(json);
                if (login != null && login.ExpiresUtc > DateTime.UtcNow) return login;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Couldn't read sign-in: {ex.Message}");
            }

            Clear();
            return null;
        }

        public static void Clear()
        {
            try
            {
                SecureStorage.Default.Remove(StorageKey);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Couldn't clear sign-in: {ex.Message}");
            }
        }
    }
}
