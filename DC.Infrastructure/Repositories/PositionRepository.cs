using DC.Domain.Entities;
using DC.Domain.Interfaces;
using DC.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DC.Infrastructure.Repositories
{
    public class PositionRepository : IPositionRepository
    {
        private readonly DepthChartDbContext _context;
        private readonly ILogger<PositionRepository> _logger;

        public PositionRepository(DepthChartDbContext context, ILogger<PositionRepository> logger)
        {
            _context = context;
            _logger = logger;
        }
        
        // Get a specific Position by ID
        public async Task<Position?> GetByIdAsync(int id)
        {
            _logger.LogInformation($"Retrieving player with ID: {id}");
            return await _context.Positions
                                 .Include(p => p.Orders)
                                 .FirstOrDefaultAsync(p => p.PositionId == id);
        }

        // Get all Positions
        public async Task<List<Position>> GetAllAsync()
        {
            _logger.LogInformation("Retrieving all positions");
            return await _context.Positions.Include(p => p.Orders)
                                           .ToListAsync();
        }

        // Add a new Position
        public async Task AddAsync(Position position)
        {
            _logger.LogInformation("Adding a new position");
            await _context.Positions.AddAsync(position);
        }

        // Update an existing Position
        public async Task UpdateAsync(Position position)
        {
            _logger.LogInformation($"Updating position with ID: {position.PositionId}");
            _context.Positions.Update(position);
        }

        // Delete a Position by ID
        public async Task DeleteAsync(int id)
        {
            _logger.LogInformation($"Deleting position with ID: {id}");
            var position = await _context.Positions.FindAsync(id);
            if (position != null)
            {
                _logger.LogWarning($"Delete with ID: {id} not found for deletion");
                _context.Positions.Remove(position);
            }
        }

        // Save changes to the database
        public async Task SaveChangesAsync()
        {
            _logger.LogInformation("Saving changes to the database");
            await _context.SaveChangesAsync();
        }

        public async Task<(Position?, bool)> GetByPositionNameAndTeamIdAsync(string positionName, int teamId)
        {
            var team = await _context.Teams.FindAsync(teamId);
            if (team != null)
            {
                return (await _context.Positions.Where(x => x.Name == positionName && x.TeamId == teamId).FirstOrDefaultAsync(), true);
            }

            return (null, false);
        }

        async Task<(List<Position>?, bool)> IPositionRepository.GetFullDepthChart(int teamId)
        {
            var team = await _context.Teams.FindAsync(teamId);
            if (team != null)
            {
                return (await _context.Positions.Where(x => x.TeamId == teamId).ToListAsync(), true);
            }

            return (null, false);
        }
    }
}