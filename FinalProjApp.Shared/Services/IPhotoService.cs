using FinalProjApp.Models;

namespace FinalProjApp.Services
{
    /// <summary>
    /// The photo gallery: a public Showcase album guests see, and a private Families
    /// album where staff see everything and parents see only photos of their own kids.
    /// Only admins add, tag, move or delete.
    /// </summary>
    public interface IPhotoService
    {
        /// <summary>The album as <paramref name="viewer"/> may see it (null for a guest).</summary>
        Task<List<Photo>> GetAsync(bool publicAlbum, AppUser? viewer);
        Task<Photo> AddAsync(string fileName, string contentType, byte[] data, bool publicAlbum, IReadOnlyCollection<int> childIds);

        /// <summary>Replaces the kids tagged in a photo.</summary>
        Task SetKidsAsync(int photoId, IReadOnlyCollection<int> childIds);
        Task MoveAsync(int photoId, bool toPublicAlbum);
        Task DeleteAsync(int photoId);
    }
}
