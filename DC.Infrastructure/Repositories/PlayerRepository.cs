using DC.Domain.Entities;
using DC.Domain.Interfaces;
using DC.Domain.Logging;
using DC.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DC.Infrastructure.Repositories
{
    public class PlayerRepository : IPlayerRepository
    {
        private readonly DepthChartDbContext _context;
        private readonly IAppLogger<PlayerRepository> _logger;

        public PlayerRepository(DepthChartDbContext context, IAppLogger<PlayerRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Get a specific Player by ID
        public async Task<Player?> GetByIdAsync(int id)
        {
            _logger.LogInformation($"Retrieving player with ID: {id}");
            return await _context.Players
                                 .Include(p => p.Team) // Include the Team related to the Player
                                 .Include(p => p.Orders) // Include the Position of the Player
                                 .FirstOrDefaultAsync(p => p.PlayerId == id);
        }

        // Get all Players
        public async Task<List<Player>> GetAllAsync()
        {
            _logger.LogInformation("Retrieving all players");
            return await _context.Players
                                 .Include(p => p.Team) // Include the Team related to the Players
                                 .Include(p => p.Orders) // Include the Positions of the Players
                                 .ToListAsync();
        }

        // Add a new Player
        public async Task AddAsync(Player player)
        {
            _logger.LogInformation("Adding a new player");
            await _context.Players.AddAsync(player);
        }

        // Update an existing Player
        public async Task UpdateAsync(Player player)
        {
            _logger.LogInformation($"Updating player with ID: {player.PlayerId}");
            _context.Players.Update(player);
        }

        // Delete a Player by ID
        public async Task DeleteAsync(int id)
        {
            _logger.LogInformation($"Deleting player with ID: {id}");
            var player = await _context.Players.FindAsync(id);
            if (player != null)
            {
                _logger.LogWarning($"Player with ID: {id} not found for deletion");
                _context.Players.Remove(player);
            }
        }

        // Save changes to the database
        public async Task SaveChangesAsync()
        {
            _logger.LogInformation("Saving changes to the database");
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Find a Player by Player Number and a teamId
        /// </summary>
        /// <param name="playerNumber">A player number is unique in a team (player number is defined during the player entry)</param>
        /// <param name="teamId">teamId was generated during the team creation</param>
        /// <returns>First Item is a Player and the other one checks the given teamId exists or not</returns>
        public async Task<(Player?, bool)> GetByPlayerNumberAndTeamIdAsync(int playerNumber, int teamId)
        {
            var team = await _context.Teams.FindAsync(teamId);
            if (team != null)
            {
                return (await _context.Players.Where(x => x.Number == playerNumber && x.TeamId == teamId).FirstOrDefaultAsync(), true);
            }

            return (null, false);
        }
    }
}