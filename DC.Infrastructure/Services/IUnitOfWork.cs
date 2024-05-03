using DC.Domain.Entities;

namespace DC.Infrastructure.Services
{
    public interface IUnitOfWork
    {
        Task<int> SaveChangesAsync();

        // Use case 1: Add a player to the Depth Chart
        Task AddPlayerToDepthChart(string positionName, int playerNumber, int? depthPosition, int teamId = 1);

        // Use case 2: Remove a player from the Depth Chart
        Task<List<(int, string)>> RemovePlayerFromDepthChart(string positionName, int playerNumber, int teamId = 1);

        // Use case 3: Get the Backups list from the Depth Chart
        Task<List<(int, string)>> GetBackups(string positionName, int playerNumber, int teamId = 1);

        // Use case 4: Get the full Depth Chart
        Task<IDictionary<string, List<(int, string)>>> GetFullDepthChart(int teamId = 1);

        Task AddSportAsync(Sport sport);
    }
}