using DC.Domain.Entities;
using DC.Domain.Interfaces;
using DC.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Xml.Linq;

namespace DC.Infrastructure.Repositories
{
    public class TeamRepository : ITeamRepository
    {
        private readonly DepthChartDbContext _context;
        private readonly ILogger<TeamRepository> _logger;

        public TeamRepository(DepthChartDbContext context, ILogger<TeamRepository> logger)
        {
            _context = context;
            _logger = logger;
        }
        
        // Get a specific Team by ID
        public async Task<Team?> GetByIdAsync(int id)
        {
            _logger.LogInformation($"Retrieving team with ID: {id}");
            return await _context.Teams.Include(t => t.Players)
                                        .FirstOrDefaultAsync(t => t.TeamId == id);
        }

        // Get all Teams
        public async Task<List<Team>> GetAllAsync()
        {
            _logger.LogInformation("Retrieving all teams");
            return await _context.Teams.Include(t => t.Players)
                                       .ToListAsync();
        }

        // Add a new Team
        public async Task AddAsync(Team team)
        {
            _logger.LogInformation("Adding a new team");
            await _context.Teams.AddAsync(team);
        }

        // Update an existing Team
        public async Task UpdateAsync(Team team)
        {
            _logger.LogInformation($"Updating team with ID: {team.TeamId}");
            _context.Teams.Update(team);
        }

        // Delete a Team by ID
        public async Task DeleteAsync(int id)
        {
            _logger.LogInformation($"Deleting team with ID: {id}");
            var team = await _context.Teams.FindAsync(id);
            if (team != null)
            {
                _logger.LogWarning($"Team with ID: {id} not found for deletion");
                _context.Teams.Remove(team);
            }
        }

        // Save changes to the database
        public async Task SaveChangesAsync()
        {
            _logger.LogInformation("Saving changes to the database");
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Find a Team by Team Name and a sportId
        /// </summary>
        /// <param name="teamName">Assumption: A team name is unique under a sport (team name is defined during the team entry)</param>
        /// <param name="sportId">sportId was generated during the sport creation</param>
        /// <returns>First Item is a Team and the other one checks the given sportId exists or not</returns>
        public async Task<(Team?, bool)> GetByTeamNameAndSportIdAsync(string teamName, int sportId)
        {
            var sport = await _context.Sports.FindAsync(sportId);
            if(sport != null)
            {
                return (await _context.Teams.Where(x => x.Name == teamName && x.SportId == sportId).FirstOrDefaultAsync(), true);
            }

            return (null, false);
        }
    }
}