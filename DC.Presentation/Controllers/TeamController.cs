using DC.Application.DTOs;
using DC.Domain.Entities;
using DC.Domain.Interfaces;
using DC.Domain.Logging;
using Microsoft.AspNetCore.Mvc;

namespace DC.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TeamController : ControllerBase
    {
        private readonly ITeamRepository _teamRepository;
        private readonly IAppLogger<TeamController> _logger;

        public TeamController(ITeamRepository teamRepository, IAppLogger<TeamController> logger)
        {
            _teamRepository = teamRepository;
            _logger = logger;
        }

        // Get all teams
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Team>>> GetAllTeams()
        {
            var teams = await _teamRepository.GetAllAsync();
            return Ok(teams);
        }

        // Get a specific team by ID
        [HttpGet("{id}")]
        public async Task<ActionResult<Team>> GetTeamById(int id)
        {
            var team = await _teamRepository.GetByIdAsync(id);
            if (team == null)
            {
                return NotFound();
            }
            return Ok(team);
        }

        // Add a new team
        [HttpPost]
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
        public async Task<ActionResult> UpdateTeam(int id, [FromBody] Team updatedTeam)
        {
            if (id != updatedTeam.TeamId)
            {
                return BadRequest();
            }

            var existingTeam = await _teamRepository.GetByIdAsync(id);
            if (existingTeam == null)
            {
                return NotFound();
            }

            await _teamRepository.UpdateAsync(updatedTeam);
            await _teamRepository.SaveChangesAsync();
            return NoContent();
        }

        // Delete a team by ID
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteTeam(int id)
        {
            var existingTeam = await _teamRepository.GetByIdAsync(id);
            if (existingTeam == null)
            {
                return NotFound();
            }

            await _teamRepository.DeleteAsync(id);
            await _teamRepository.SaveChangesAsync();
            return NoContent();
        }
    }
}