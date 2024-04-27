using DC.Domain.Entities;

namespace DC.Domain.Interfaces
{
    public interface IOrderRepository : IRepository<Order>
    {
        Task<Order?> GetByIdAsync(int positionId, int playerId);
        void Delete(int positionId, int playerId);

        // Use case 1: Add a player to the Depth Chart
        Task AddPlayerToDepthChart(int positionId, int playerId, int? depthPosition);

        // Use case 2: Remove a player from the Depth Chart
        Task<Player>? RemovePlayerToDepthChart(int positionId, int playerId);

        // Use case 3: Get the Backups list from the Depth Chart
        Task<Player>? GetBackups(int positionId, int playerId);        
    }
}