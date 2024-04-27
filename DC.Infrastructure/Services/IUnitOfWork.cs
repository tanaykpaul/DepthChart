using DC.Domain.Entities;
using DC.Domain.Interfaces;

namespace DC.Infrastructure.Services
{
    public interface IUnitOfWork : IDisposable
    {
        ISportRepository SportRepository { get; }
        ITeamRepository TeamRepository { get; }
        IPositionRepository PositionRepository { get; }
        IPlayerRepository PlayerRepository { get; }
        IOrderRepository OrderRepository { get; }

        Task<int> SaveChangesAsync();

        // Use case 1: Add a player to the Depth Chart
        Task<Order?> AddPlayerToDepthChart(string positionName, int playerNumber, int? depthPosition, int teamId = 1);

        // Use case 2: Remove a player from the Depth Chart
        Task<Player?> RemovePlayerFromDepthChart(string positionName, int playerNumber, int teamId = 1);

        // Use case 3: Get the Backups list from the Depth Chart
        Task<ICollection<Player>> GetBackups(int positionId, int playerId);
    }
}