using DC.Domain.Entities;

namespace DC.Domain.Interfaces
{
    public interface IPositionRepository : IOneToManyRepository<Position>, IRepository<Position>
    {
        // First Item of the Return is a Position and the other one checks the given teamId exists or not
        Task<(Position?, bool)> GetByPositionNameAndTeamIdAsync(string positionName, int teamId);
    }
}