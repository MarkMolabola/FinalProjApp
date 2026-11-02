using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace FinalProjApp.Services.Api
{
    /// <summary>
    /// Talks to FinalProjApp.Web's /api. Holds the sign-in token for the life of the
    /// app (signing out, or restarting the app, clears it, as before).
    /// </summary>
    public class ApiClient
    {
        /// <summary>
        /// Debug builds use the web app running on this computer (10.0.2.2 is how the
        /// Android emulator reaches it). Release builds use the hosted site.
        /// </summary>
        public static Uri BaseAddress =>
#if DEBUG
            DeviceInfo.Platform == DevicePlatform.Android
                ? new Uri("http://10.0.2.2:5208/")
                : new Uri("http://localhost:5208/");
#else
            new Uri("https://calybear.azurewebsites.net/"); // TODO: set once deployed to Azure
#endif

        private readonly HttpClient _http = new() { BaseAddress = BaseAddress, Timeout = TimeSpan.FromSeconds(30) };

        public string? Token
        {
            get => _http.DefaultRequestHeaders.Authorization?.Parameter;
            set => _http.DefaultRequestHeaders.Authorization =
                value == null ? null : new AuthenticationHeaderValue("Bearer", value);
        }

        public async Task<T> GetAsync<T>(string path) =>
            await _http.GetFromJsonAsync<T>(path) ?? throw new HttpRequestException($"Empty reply from {path}");

        public async Task<HttpResponseMessage> PostAsync<T>(string path, T body) =>
            await _http.PostAsJsonAsync(path, body);

        public async Task SendAsync<T>(HttpMethod method, string path, T body) =>
            (await _http.SendAsync(new HttpRequestMessage(method, path) { Content = JsonContent.Create(body) }))
                .EnsureSuccessStatusCode();

        public async Task DeleteAsync(string path) =>
            (await _http.DeleteAsync(path)).EnsureSuccessStatusCode();
    }
}
