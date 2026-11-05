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
        /// Debug builds use the web app running on this computer. The Android emulator
        /// reaches it at 10.0.2.2; a real phone on USB uses localhost after
        /// `adb reverse tcp:5208 tcp:5208`. Release builds use the hosted site.
        /// </summary>
        public static Uri BaseAddress =>
#if DEBUG
            DeviceInfo.Platform == DevicePlatform.Android && DeviceInfo.DeviceType == DeviceType.Virtual
                ? new Uri("http://10.0.2.2:5208/")
                : new Uri("http://localhost:5208/");
#else
            new Uri("https://calybear.azurewebsites.net/"); // TODO: set once deployed to Azure
#endif

        private readonly HttpClient _http = new(new NetworkErrorHandler { InnerHandler = new HttpClientHandler() })
        {
            BaseAddress = BaseAddress,
            Timeout = TimeSpan.FromSeconds(30)
        };

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

        /// <summary>Sends a request and hands back the reply as-is, for callers that read error replies.</summary>
        public Task<HttpResponseMessage> SendRawAsync(HttpMethod method, string path, object? body) =>
            _http.SendAsync(new HttpRequestMessage(method, path) { Content = body == null ? null : JsonContent.Create(body) });

        public async Task DeleteAsync(string path) =>
            (await _http.DeleteAsync(path)).EnsureSuccessStatusCode();

        /// <summary>
        /// Android reports some connection failures (server not running, connection dropped)
        /// as Java IOExceptions instead of HttpRequestException. Rewrap them so every caller's
        /// "can't reach Caly Bear" handling catches them.
        /// </summary>
        private sealed class NetworkErrorHandler : DelegatingHandler
        {
            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                try
                {
                    return await base.SendAsync(request, cancellationToken);
                }
                catch (Exception ex) when (IsConnectionFailure(ex))
                {
                    throw new HttpRequestException(ex.Message, ex);
                }
            }

            private static bool IsConnectionFailure(Exception ex) =>
                ex is System.IO.IOException
#if ANDROID
                || ex is Java.IO.IOException
#endif
                ;
        }
    }
}
