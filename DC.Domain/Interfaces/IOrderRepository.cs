using DC.Domain.Entities;

namespace DC.Domain.Interfaces
{
    public interface IOrderRepository : IRepository<Order>
    {
        // First Item of the Return is an Order and the other one checks the given playerId and positionId exist or not
        Task<(Order?, bool)> GetByPlayerIdAndPositionIdAsync(int playerId, int positionId);

        // Use case 1: Add a player to the Depth Chart
        Task<int?> AddPlayerToDepthChart(int positionId, int playerId, int? depthPosition);

        // Use case 2: Remove a player from the Depth Chart
        Task<Player>? RemovePlayerToDepthChart(int positionId, int playerId);

        // Use case 3: Get the Backups list from the Depth Chart
        Task<Player>? GetBackups(int positionId, int playerId);        
    }
}