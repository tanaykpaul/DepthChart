using DC.Application.DTOs;
using DC.Domain.Entities;
using DC.Domain.Interfaces;
using DC.Domain.Logging;
using DC.Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace DC.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TeamController : ControllerBase
    {
        private readonly ITeamRepository _teamRepository;
        private readonly IAppLogger _logger;

        public TeamController(ITeamRepository teamRepository, IAppLogger logger)
        {
            _teamRepository = teamRepository;
            _logger = logger;
        }

        // Get all teams
        [HttpGet("allTeams")]
        public async Task<ActionResult<IEnumerable<TeamCreationResponseDTO>>> GetAllTeams()
        {
            _logger.LogInformation($"Finding all teams...");
            var teams = await _teamRepository.GetAllAsync();
            _logger.LogInformation($"There are {teams.Count} teams returned.");

            var teamsDtoCollection = new List<TeamCreationResponseDTO>();
            foreach (var team in teams)
            {
                teamsDtoCollection.Add(new TeamCreationResponseDTO { TeamId = team.TeamId, TeamName = team.Name, SportId = team.SportId });
            }
            return Ok(teamsDtoCollection);
        }

        // Get a specific team by ID
        [HttpGet("{id}")]
        public async Task<ActionResult<TeamCreationResponseDTO>> GetTeamById(int id)
        {
            _logger.LogInformation($"Finding a team with Id {id}.");
            var team = await _teamRepository.GetByIdAsync(id);
            if (team == null)
            {
                _logger.LogWarning($"No team is found with Id {id}.");
                return NotFound($"No team is found with Id {id}.");
            }
            return Ok(new TeamCreationResponseDTO { TeamId = team.TeamId, TeamName = team.Name, SportId = team.SportId });
        }

        // Add a new team
        [HttpPost("addSport")]
        public async Task<ActionResult<TeamCreationResponseDTO>> AddTeam([FromBody] TeamDTO teamDto)
        {
            var teamItem = await _teamRepository.GetByTeamNameAndSportIdAsync(teamDto.Name, teamDto.SportId);
            if (!teamItem.Item2)
            {
                return BadRequest($"There is no sport item is crreated yet by SportId = {teamDto.SportId}.");
            }
            if (teamItem.Item1 != null)
            {
                return BadRequest($"There is a team exists with the team name {teamDto.Name} under the sport Id {teamDto.SportId}");
            }

            var team = new Team
            {
                Name = teamDto.Name,
                SportId = teamDto.SportId
            };

            await _teamRepository.AddAsync(team);
            await _teamRepository.SaveChangesAsync();
            return CreatedAtAction(nameof(GetTeamById), new { id = team.TeamId }, new TeamCreationResponseDTO { TeamId = team.TeamId, SportId = team.SportId });
        }

        // Update an existing team
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateTeam(int id, [FromBody] TeamDTO teamDTO)
        {
            _logger.LogInformation($"Updating a team by id {id}.");
            var team = await _teamRepository.GetByIdAsync(id);
            if (team == null)
            {
                _logger.LogWarning($"No team is found with Id {id}.");
                return BadRequest($"There is a team exists with id {id}");
            }

            team.Name = teamDTO.Name;

            await _teamRepository.UpdateAsync(team);
            await _teamRepository.SaveChangesAsync();
            return NoContent();
        }

        // Delete a team by ID
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteTeam(int id)
        {
            _logger.LogInformation($"Deleting a team by id {id}.");
            var team = await _teamRepository.GetByIdAsync(id);
            if (team == null)
            {
                _logger.LogWarning($"No team is found with Id {id}.");
                return BadRequest($"There is a team exists with id {id}");
            }

            await _teamRepository.DeleteAsync(id);
            await _teamRepository.SaveChangesAsync();
            return NoContent();
        }
    }
}