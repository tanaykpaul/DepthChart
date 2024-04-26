using DC.Domain.Entities;

namespace DC.Domain.Interfaces
{
    public interface IPositionRepository : IRepository<Position>
    {
        // First Item of the Return is a Position and the other one checks the given teamId exists or not
        Task<(Position?, bool)> GetByPositionNameAndTeamIdAsync(string positionName, int teamId);

        // Use case 4: Get the full Depth Chart; The second Item of the Return checks the given teamId exists or not 
        Task<(List<Position>?, bool)> GetFullDepthChart(int teamId);
    }
}