using FinalProjApp.Models;
using System.Net.Http.Json;

namespace FinalProjApp.Services.Api
{
    // The phone app's data services: the same ICRUD / IPhotoService the shared
    // pages use, backed by the web app's API instead of a database on the phone.

    public class ApiChildService : ICRUD<Child>
    {
        private readonly ApiClient _api;
        public ApiChildService(ApiClient api) => _api = api;

        public Task<List<Child>> GetList() => _api.GetAsync<List<Child>>("api/children");
        public async Task Add(Child child) => (await _api.PostAsync("api/children", child)).EnsureSuccessStatusCode();
        public Task Update(int id, Child child) => _api.SendAsync(HttpMethod.Put, $"api/children/{id}", child);
        public Task Delete(Child child) => _api.DeleteAsync($"api/children/{child.ChildId}");

        public Task Update(Guid id, Child child) => throw new NotSupportedException("Kids are updated by number.");
        public Task<Child> GetbyID(Guid id) => throw new NotSupportedException("Kids are looked up by number.");
    }

    public class ApiGuardianService : ICRUD<Guardian>
    {
        private readonly ApiClient _api;
        public ApiGuardianService(ApiClient api) => _api = api;

        public Task<List<Guardian>> GetList() => _api.GetAsync<List<Guardian>>("api/guardians");
        public Task<Guardian> GetbyID(Guid id) => _api.GetAsync<Guardian>($"api/guardians/{id}");
        public async Task Add(Guardian guardian) => (await _api.PostAsync("api/guardians", guardian)).EnsureSuccessStatusCode();
        public Task Update(Guid id, Guardian guardian) => _api.SendAsync(HttpMethod.Put, $"api/guardians/{id}", guardian);
        public Task Delete(Guardian guardian) => _api.DeleteAsync($"api/guardians/{guardian.GuardianId}");

        public Task Update(int id, Guardian guardian) => throw new NotSupportedException("Guardians are updated by id.");
    }

    public class ApiScheduleEventService : ICRUD<ScheduleEvent>
    {
        private readonly ApiClient _api;
        public ApiScheduleEventService(ApiClient api) => _api = api;

        public Task<List<ScheduleEvent>> GetList() => _api.GetAsync<List<ScheduleEvent>>("api/events");
        public async Task Add(ScheduleEvent ev) => (await _api.PostAsync("api/events", ev)).EnsureSuccessStatusCode();
        public Task Update(int id, ScheduleEvent ev) => _api.SendAsync(HttpMethod.Put, $"api/events/{id}", ev);
        public Task Delete(ScheduleEvent ev) => _api.DeleteAsync($"api/events/{ev.Id}");

        public Task Update(Guid id, ScheduleEvent ev) => throw new NotSupportedException("Events are updated by number.");
        public Task<ScheduleEvent> GetbyID(Guid id) => throw new NotSupportedException("Events are looked up by number.");
    }

    public class ApiPhotoService : IPhotoService
    {
        private readonly ApiClient _api;
        public ApiPhotoService(ApiClient api) => _api = api;

        public Task<List<Photo>> GetAsync(bool publicAlbum) =>
            _api.GetAsync<List<Photo>>(publicAlbum ? "api/photos/showcase" : "api/photos/families");

        public async Task<Photo> AddAsync(string fileName, string contentType, byte[] data, bool publicAlbum)
        {
            var photo = new Photo { FileName = fileName, ContentType = contentType, Data = data, IsPublic = publicAlbum };
            var response = await _api.PostAsync("api/photos", photo);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Photo>() ?? photo;
        }

        public Task MoveAsync(int photoId, bool toPublicAlbum) =>
            _api.SendAsync(HttpMethod.Put, $"api/photos/{photoId}/album?isPublic={toPublicAlbum}", new { });

        public Task DeleteAsync(int photoId) => _api.DeleteAsync($"api/photos/{photoId}");
    }

    /// <summary>Kids page data. Problems the server explains (409s) come back as readable messages.</summary>
    public class ApiFamilyService : IFamilyService
    {
        private readonly ApiClient _api;
        public ApiFamilyService(ApiClient api) => _api = api;

        public Task<FamilySnapshot> GetAllAsync() => _api.GetAsync<FamilySnapshot>("api/family");

        // The server already limits a parent's /api/children to their own kids
        public Task<List<Child>> GetKidsOfGuardianAsync(Guid guardianId) => _api.GetAsync<List<Child>>("api/children");

        public async Task<FamilyChange> AddKidAsync(Child kid, Guardian guardian) =>
            await ReadAsync<FamilyChange>(await _api.PostAsync("api/family/kids", new NewKidRequest { Kid = kid, Guardian = guardian }));

        public Task UpdateKidAsync(Child kid) => _api.SendAsync(HttpMethod.Put, $"api/family/kids/{kid.ChildId}", kid);

        public Task DeleteKidAsync(int childId) => _api.DeleteAsync($"api/family/kids/{childId}");

        public async Task<FamilyChange> AddGuardianAsync(int childId, Guardian guardian) =>
            await ReadAsync<FamilyChange>(await _api.PostAsync($"api/family/kids/{childId}/guardians", guardian));

        public async Task UpdateGuardianAsync(Guardian guardian) =>
            await EnsureOkAsync(await _api.SendRawAsync(HttpMethod.Put, $"api/family/guardians/{guardian.GuardianId}", guardian));

        public async Task RemoveGuardianAsync(int childId, Guid guardianId) =>
            await EnsureOkAsync(await _api.SendRawAsync(HttpMethod.Delete, $"api/family/kids/{childId}/guardians/{guardianId}", new { }));

        private static async Task<T> ReadAsync<T>(HttpResponseMessage response)
        {
            await EnsureOkAsync(response);
            return (await response.Content.ReadFromJsonAsync<T>())!;
        }

        /// <summary>Turns a 409 (e.g. "A kid needs at least one guardian.") into the same error the web app shows.</summary>
        internal static async Task EnsureOkAsync(HttpResponseMessage response)
        {
            if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
                throw new InvalidOperationException((await response.Content.ReadFromJsonAsync<string>()) ?? "That change isn't allowed.");
            response.EnsureSuccessStatusCode();
        }
    }

    /// <summary>Schedule requests. The server works out who's asking from the sign-in token.</summary>
    public class ApiScheduleRequestService : IScheduleRequestService
    {
        private readonly ApiClient _api;
        public ApiScheduleRequestService(ApiClient api) => _api = api;

        public event Action? Reviewed;

        public Task<List<ScheduleRequest>> GetMineAsync(AppUser parent) =>
            _api.GetAsync<List<ScheduleRequest>>("api/schedule-requests/mine");

        public Task<List<ScheduleRequest>> GetPendingAsync(AppUser admin) =>
            _api.GetAsync<List<ScheduleRequest>>("api/schedule-requests");

        public async Task<int> CountPendingAsync(AppUser admin) => (await GetPendingAsync(admin)).Count;

        public async Task<ScheduleRequest> SubmitAsync(AppUser parent, ScheduleRequest request)
        {
            var response = await _api.PostAsync("api/schedule-requests", request);
            await ApiFamilyService.EnsureOkAsync(response);
            return (await response.Content.ReadFromJsonAsync<ScheduleRequest>())!;
        }

        public async Task ApproveAsync(AppUser admin, int requestId)
        {
            await ApiFamilyService.EnsureOkAsync(await _api.PostAsync($"api/schedule-requests/{requestId}/approve", new { }));
            Reviewed?.Invoke();
        }

        public async Task DeclineAsync(AppUser admin, int requestId, string? reason)
        {
            await ApiFamilyService.EnsureOkAsync(await _api.PostAsync($"api/schedule-requests/{requestId}/decline", new DeclineScheduleRequest { Reason = reason }));
            Reviewed?.Invoke();
        }
    }

    public class ApiInviteService : IInviteService
    {
        private readonly ApiClient _api;
        public ApiInviteService(ApiClient api) => _api = api;

        public async Task<InviteResult> SendAsync(Guid guardianId)
        {
            var response = await _api.PostAsync($"api/family/guardians/{guardianId}/invite", new { });
            response.EnsureSuccessStatusCode();
            return (await response.Content.ReadFromJsonAsync<InviteResult>())!;
        }

        public async Task<InviteDetails?> GetAsync(string code)
        {
            var response = await _api.SendRawAsync(HttpMethod.Get, $"api/invites/{Uri.EscapeDataString(code)}", null);
            return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<InviteDetails>() : null;
        }

        public async Task<string?> AcceptAsync(string code, string password)
        {
            var response = await _api.PostAsync($"api/invites/{Uri.EscapeDataString(code)}/accept", new AcceptInviteRequest { Password = password });
            return response.IsSuccessStatusCode ? null : await response.Content.ReadFromJsonAsync<string>();
        }
    }
}
