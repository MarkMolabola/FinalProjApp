using FinalProjApp.Models;

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

        public Task<List<Photo>> GetAllAsync() => _api.GetAsync<List<Photo>>("api/photos");

        public async Task<Photo> AddAsync(string fileName, string contentType, byte[] data)
        {
            var photo = new Photo { FileName = fileName, ContentType = contentType, Data = data };
            var response = await _api.PostAsync("api/photos", photo);
            response.EnsureSuccessStatusCode();
            return photo;
        }
    }
}
