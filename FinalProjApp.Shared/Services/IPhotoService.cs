using FinalProjApp.Models;

namespace FinalProjApp.Services
{
    /// <summary>The photo gallery's storage.</summary>
    public interface IPhotoService
    {
        Task<List<Photo>> GetAllAsync();
        Task<Photo> AddAsync(string fileName, string contentType, byte[] data);
    }
}
