namespace FinalProjApp.Services
{
    /// <summary>Basic data access for kids, guardians and schedule events.</summary>
    public interface ICRUD<T>
    {
        Task Add(T item);
        Task Update(Guid Id, T item);
        Task Update(int Id, T item);
        Task Delete(T item);
        Task<List<T>> GetList();
        Task<T> GetbyID(Guid ID);
    }
}
