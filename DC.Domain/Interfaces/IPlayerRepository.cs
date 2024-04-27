using DC.Domain.Entities;

namespace DC.Domain.Interfaces
{
    public interface IPlayerRepository : IOneToManyRepository<Player>, IRepository<Player>
    {
        // First Item of the Return is a Player and the other one checks the given teamId exists or not
        Task<(Player?, bool)> GetByPlayerNumberAndTeamIdAsync(int playerNumber, int teamId);
    }
}