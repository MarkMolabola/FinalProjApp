using FinalProjApp.Data;
using FinalProjApp.Models;
using Microsoft.EntityFrameworkCore;

namespace FinalProjApp.Services
{
    public class PhotoService : IPhotoService
    {
        private readonly DataContext _db;

        public PhotoService(DataContext db) => _db = db;

        public async Task<List<Photo>> GetAsync(bool publicAlbum, AppUser? viewer)
        {
            var isStaff = viewer?.Role is "Admin" or "Staff";
            var album = _db.Photos.AsNoTracking().Where(p => p.IsPublic == publicAlbum);

            if (!publicAlbum && !isStaff)
            {
                // A parent only gets photos tagged with one of their own kids
                if (viewer?.GuardianId is not { } guardianId) return new();

                var theirKids = _db.ChildGuardians.Where(l => l.GuardianId == guardianId).Select(l => l.ChildId);
                album = album.Where(p => _db.PhotoChildren.Any(l => l.PhotoId == p.Id && theirKids.Contains(l.ChildId)));
            }

            var photos = await album.OrderByDescending(p => p.Id).ToListAsync();
            if (isStaff) await FillInKidsAsync(photos);
            return photos;
        }

        public async Task<Photo> AddAsync(string fileName, string contentType, byte[] data, bool publicAlbum, IReadOnlyCollection<int> childIds)
        {
            var photo = new Photo { FileName = fileName, ContentType = contentType, Data = data, IsPublic = publicAlbum };
            _db.Photos.Add(photo);
            await _db.SaveChangesAsync();

            await SetKidsAsync(photo.Id, childIds);
            photo.ChildIds = childIds.ToList();
            return photo;
        }

        public async Task SetKidsAsync(int photoId, IReadOnlyCollection<int> childIds)
        {
            if (!await _db.Photos.AnyAsync(p => p.Id == photoId)) return;

            var realKids = await _db.children.Where(c => childIds.Contains(c.ChildId)).Select(c => c.ChildId).ToListAsync();

            var current = await _db.PhotoChildren.Where(l => l.PhotoId == photoId).ToListAsync();
            _db.PhotoChildren.RemoveRange(current.Where(l => !realKids.Contains(l.ChildId)));
            _db.PhotoChildren.AddRange(realKids
                .Where(childId => current.All(l => l.ChildId != childId))
                .Select(childId => new PhotoChild { PhotoId = photoId, ChildId = childId }));
            await _db.SaveChangesAsync();
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
            _db.PhotoChildren.RemoveRange(_db.PhotoChildren.Where(l => l.PhotoId == photoId));
            _db.Photos.Remove(photo);
            await _db.SaveChangesAsync();
        }

        private async Task FillInKidsAsync(List<Photo> photos)
        {
            var photoIds = photos.Select(p => p.Id).ToList();
            var links = await _db.PhotoChildren.AsNoTracking().Where(l => photoIds.Contains(l.PhotoId)).ToListAsync();
            foreach (var photo in photos)
                photo.ChildIds = links.Where(l => l.PhotoId == photo.Id).Select(l => l.ChildId).ToList();
        }
    }
}
