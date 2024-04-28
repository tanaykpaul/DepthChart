using DC.Application.DTOs;
using DC.Domain.Entities;
using DC.Domain.Interfaces;
using DC.Domain.Logging;
using DC.Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DC.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PositionController : ControllerBase
    {
        private readonly IPositionRepository _positionRepository;
        private readonly IAppLogger _logger;

        public PositionController(IPositionRepository positionRepository, IAppLogger logger)
        {
            _positionRepository = positionRepository;
            _logger = logger;
        }

        // Get all positions
        [HttpGet("allPositions")]
        public async Task<ActionResult<IEnumerable<PositionCreationResponseDTO>>> GetAllPositions()
        {
            _logger.LogInformation($"Finding all positions...");
            var positions = await _positionRepository.GetAllAsync();
            _logger.LogInformation($"There are {positions.Count} positions returned.");

            var positionsDtoCollection = new List<PositionCreationResponseDTO>();
            foreach (var position in positions)
            {
                positionsDtoCollection.Add(new PositionCreationResponseDTO
                {
                    PositionId = position.PositionId,
                    PositionName = position.Name,
                    TeamId = position.TeamId
                });
            }
            return Ok(positionsDtoCollection);
        }

        // Get a specific position by ID
        [HttpGet("{id}")]
        public async Task<ActionResult<PositionCreationResponseDTO>> GetPositionById(int id)
        {
            _logger.LogInformation($"Finding a position with Id {id}.");
            var position = await _positionRepository.GetByIdAsync(id);
            if (position == null)
            {
                _logger.LogWarning($"No position is found with Id {id}.");
                return NotFound($"No position is found with Id {id}.");
            }
            return Ok(new PositionCreationResponseDTO
            {
                PositionId = position.PositionId,
                PositionName = position.Name,
                TeamId = position.TeamId
            });
        }

        // Add a new position
        [HttpPost("addPosition")]
        public async Task<ActionResult<PositionCreationResponseDTO>> AddPosition([FromBody] PositionDTO positionDto)
        {
            var positionItem = await _positionRepository.GetByPositionNameAndTeamIdAsync(positionDto.Name, positionDto.TeamId);
            if (!positionItem.Item2)
            {
                return BadRequest($"There is no team item is crreated yet by TeamId = {positionDto.TeamId}.");
            }
            if (positionItem.Item1 != null)
            {
                return BadRequest($"There is a player exists with the player number {positionItem.Item1.Name} under the team Id = {positionItem.Item1.TeamId}");
            }

            var position = new Position
            {
                Name = positionDto.Name,
                TeamId = positionDto.TeamId
            };

            await _positionRepository.AddAsync(position);
            await _positionRepository.SaveChangesAsync();
            return CreatedAtAction(nameof(GetPositionById), new { id = position.PositionId }, new PositionCreationResponseDTO { PositionId = position.PositionId, TeamId = position.TeamId });
        }

        // Update an existing position
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdatePosition(int id, [FromBody] PositionDTO updatedPosition)
        {
            _logger.LogInformation($"Updating a position by id {id}.");
            var position = await _positionRepository.GetByIdAsync(id);
            if (position == null)
            {
                _logger.LogWarning($"No position is found with Id {id}.");
                return BadRequest($"There is no position exists with id {id}");
            }

            position.Name = updatedPosition.Name;
            
            await _positionRepository.UpdateAsync(position);
            await _positionRepository.SaveChangesAsync();
            return NoContent();
        }

        // Delete a position by ID
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeletePosition(int id)
        {
            _logger.LogInformation($"Deleting a position by id {id}.");
            var position = await _positionRepository.GetByIdAsync(id);
            if (position == null)
            {
                _logger.LogWarning($"No position is found with Id {id}.");
                return BadRequest($"There is no position exists with id {id}");
            }

            await _positionRepository.DeleteAsync(id);
            await _positionRepository.SaveChangesAsync();
            return NoContent();
        }
    }
}