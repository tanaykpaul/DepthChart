using DC.Domain.Entities;

namespace DC.Domain.Interfaces
{
    public interface ISportRepository : IRepository<Sport>
    {
        Task<Sport?> GetByNameAsync(string name);
    }
}