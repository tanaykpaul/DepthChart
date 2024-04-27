namespace DC.Domain.Interfaces
{
    public interface IOneToManyRepository<T>
    {
        Task<T?> GetByIdAsync(int id);
        Task DeleteAsync(int id);
    }
}