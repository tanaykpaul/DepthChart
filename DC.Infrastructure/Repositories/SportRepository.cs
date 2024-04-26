using DC.Domain.Entities;
using DC.Domain.Interfaces;
using DC.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DC.Infrastructure.Repositories
{
    public class SportRepository : ISportRepository
    {
        private readonly DepthChartDbContext _context;
        private readonly ILogger<SportRepository> _logger;

        public SportRepository(DepthChartDbContext context, ILogger<SportRepository> logger)
        {
            _context = context;
            _logger = logger;
        }
        
        // Get a specific Sport by ID
        public async Task<Sport?> GetByIdAsync(int id)
        {
            _logger.LogInformation($"Retrieving sport with ID: {id}");
            return await _context.Sports.FirstOrDefaultAsync(s => s.SportId == id);
        }

        // Get all Sports
        public async Task<List<Sport>> GetAllAsync()
        {
            _logger.LogInformation("Retrieving all sports");
            return await _context.Sports.ToListAsync();
        }

        // Add a new Sport
        public async Task AddAsync(Sport sport)
        {
            _logger.LogInformation("Adding a new sport");
            await _context.Sports.AddAsync(sport);
        }

        // Update an existing Sport
        public async Task UpdateAsync(Sport sport)
        {
            _logger.LogInformation($"Updating sport with ID: {sport.SportId}");
            _context.Sports.Update(sport);
        }

        // Delete a Sport by ID
        public async Task DeleteAsync(int id)
        {
            _logger.LogInformation($"Deleting sport with ID: {id}");
            var sport = await _context.Sports.FindAsync(id);
            if (sport != null)
            {
                _logger.LogWarning($"Sport with ID: {id} not found for deletion");
                _context.Sports.Remove(sport);
            }
        }

        // Save changes to the database
        public async Task SaveChangesAsync()
        {
            _logger.LogInformation("Saving changes to the database");
            await _context.SaveChangesAsync();
        }

        public async Task<Sport?> GetByNameAsync(string name)
        {
            return await _context.Sports.Where(x => x.Name == name).FirstOrDefaultAsync();
        }
    }
}