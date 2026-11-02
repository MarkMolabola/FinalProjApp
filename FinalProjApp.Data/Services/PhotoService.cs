using FinalProjApp.Data;
using FinalProjApp.Models;
using Microsoft.EntityFrameworkCore;

namespace FinalProjApp.Services
{
    public class PhotoService : IPhotoService
    {
        private readonly DataContext _db;

        public PhotoService(DataContext db) => _db = db;

        public Task<List<Photo>> GetAllAsync() =>
            _db.Photos.OrderByDescending(p => p.Id).ToListAsync();

        public async Task<Photo> AddAsync(string fileName, string contentType, byte[] data)
        {
            var photo = new Photo { FileName = fileName, ContentType = contentType, Data = data };
            _db.Photos.Add(photo);
            await _db.SaveChangesAsync();
            return photo;
        }
    }
}
