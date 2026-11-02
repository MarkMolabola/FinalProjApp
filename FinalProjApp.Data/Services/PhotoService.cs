using FinalProjApp.Data;
using FinalProjApp.Models;
using Microsoft.EntityFrameworkCore;

namespace FinalProjApp.Services
{
    public class PhotoService : IPhotoService
    {
        private readonly DataContext _db;

        public PhotoService(DataContext db) => _db = db;

        public Task<List<Photo>> GetAsync(bool publicAlbum) =>
            _db.Photos.AsNoTracking()
                .Where(p => p.IsPublic == publicAlbum)
                .OrderByDescending(p => p.Id)
                .ToListAsync();

        public async Task<Photo> AddAsync(string fileName, string contentType, byte[] data, bool publicAlbum)
        {
            var photo = new Photo { FileName = fileName, ContentType = contentType, Data = data, IsPublic = publicAlbum };
            _db.Photos.Add(photo);
            await _db.SaveChangesAsync();
            return photo;
        }

        public async Task MoveAsync(int photoId, bool toPublicAlbum)
        {
            if (await _db.Photos.FindAsync(photoId) is not { } photo) return;
            photo.IsPublic = toPublicAlbum;
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(int photoId)
        {
            if (await _db.Photos.FindAsync(photoId) is not { } photo) return;
            _db.Photos.Remove(photo);
            await _db.SaveChangesAsync();
        }
    }
}
