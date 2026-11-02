using FinalProjApp.Models;

namespace FinalProjApp.Services
{
    /// <summary>
    /// The photo gallery: a public Showcase album guests see, and a private Families
    /// album for signed-in parents and staff. Only admins add, move or delete.
    /// </summary>
    public interface IPhotoService
    {
        Task<List<Photo>> GetAsync(bool publicAlbum);
        Task<Photo> AddAsync(string fileName, string contentType, byte[] data, bool publicAlbum);
        Task MoveAsync(int photoId, bool toPublicAlbum);
        Task DeleteAsync(int photoId);
    }
}
