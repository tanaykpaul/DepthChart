using DC.Domain.Entities;

namespace DC.Domain.Interfaces
{
    public interface ISportRepository : IOneToManyRepository<Sport>, IRepository<Sport>
    {
        Task<Sport?> GetByNameAsync(string name);
    }
}