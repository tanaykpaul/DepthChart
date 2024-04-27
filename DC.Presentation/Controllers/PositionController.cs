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
    public class PositionController : ControllerBase
    {
        private readonly IPositionRepository _positionRepository;
        private readonly IAppLogger<PositionController> _logger;

        public PositionController(IPositionRepository positionRepository, IAppLogger<PositionController> logger)
        {
            _positionRepository = positionRepository;
            _logger = logger;
        }

        // Get all positions
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Position>>> GetAllPositions()
        {
            var positions = await _positionRepository.GetAllAsync();
            return Ok(positions);
        }

        // Get a specific position by ID
        [HttpGet("{id}")]
        public async Task<ActionResult<Position>> GetPositionById(int id)
        {
            var position = await _positionRepository.GetByIdAsync(id);
            if (position == null)
            {
                return NotFound();
            }
            return Ok(position);
        }

        // Add a new position
        [HttpPost]
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

        // Add a new order for Use case 4
        [HttpPost("UseCase4")]
        public async Task<ActionResult<PositionCreationResponseDTO>> GetFullDepthChart([FromBody] PositionDTO positionDto)
        {
            var positionItem = await _positionRepository.GetFullDepthChart(positionDto.TeamId);
            if (!positionItem.Item2)
            {
                return BadRequest($"There is no team item is crreated yet by TeamId = {positionDto.TeamId}.");
            }
            if (positionItem.Item1 != null)
            {
                // return the full depth chart
                return Ok(positionItem.Item1);
            }
            else
            {
                return Ok(null);
            }
        }

        // Update an existing position
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdatePosition(int id, [FromBody] Position updatedPosition)
        {
            if (id != updatedPosition.PositionId)
            {
                return BadRequest();
            }

            var existingPosition = await _positionRepository.GetByIdAsync(id);
            if (existingPosition == null)
            {
                return NotFound();
            }

            await _positionRepository.UpdateAsync(updatedPosition);
            await _positionRepository.SaveChangesAsync();
            return NoContent();
        }

        // Delete a position by ID
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeletePosition(int id)
        {
            var existingPosition = await _positionRepository.GetByIdAsync(id);
            if (existingPosition == null)
            {
                return NotFound();
            }

            await _positionRepository.DeleteAsync(id);
            await _positionRepository.SaveChangesAsync();
            return NoContent();
        }
    }
}