using DC.Domain.Entities;

namespace DC.Domain.Interfaces
{
    public interface IOrderRepository : IRepository<Order>
    {
        Task<Order?> GetByIdAsync(int positionId, int playerId);
        Task AddPlayerToDepthChart(int positionId, int playerId, int? depthPosition);
        Task<bool> RemovePlayerFromDepthChart(int positionId, int playerId);
    }
}