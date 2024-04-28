using DC.Application.DTOs;
using DC.Domain.Entities;
using DC.Domain.Interfaces;
using DC.Domain.Logging;
using Microsoft.AspNetCore.Mvc;

namespace DC.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SportController : ControllerBase
    {
        private readonly ISportRepository _sportRepository;
        private readonly IAppLogger _logger;

        public SportController(ISportRepository sportRepository, IAppLogger logger)
        {
            _sportRepository = sportRepository;
            _logger = logger;
        }

        // Get all sports
        [HttpGet("allSports")]
        public async Task<ActionResult<IEnumerable<SportCreationResponseDTO>>> GetAllSports()
        {
            _logger.LogInformation($"Finding all sports...");
            var sports = await _sportRepository.GetAllAsync();
            _logger.LogInformation($"There are {sports.Count} sports returned.");

            var sportsDtoCollection = new List<SportCreationResponseDTO>();
            foreach ( var sport in sports)
            {
                sportsDtoCollection.Add(new SportCreationResponseDTO { SportId = sport.SportId, SportName = sport.Name });
            }
            return Ok(sportsDtoCollection);
        }

        // Get a specific sport by ID
        [HttpGet("{id}")]
        public async Task<ActionResult<SportCreationResponseDTO>> GetSportById(int id)
        {
            _logger.LogInformation($"Finding a sport with Id {id}.");
            var sport = await _sportRepository.GetByIdAsync(id);
            if (sport == null)
            {
                _logger.LogWarning($"No sport is found with Id {id}.");
                return NotFound($"No sport is found with Id {id}.");
            }
            return Ok(new SportCreationResponseDTO { SportId = sport.SportId, SportName = sport.Name });
        }

        // Add a new sport
        [HttpPost("addSport")]
        public async Task<ActionResult<SportCreationResponseDTO>> AddSport([FromBody] SportDTO sportDto)
        {
            _logger.LogInformation($"Adding a sport by name {sportDto.Name}.");
            var sportItem = await _sportRepository.GetByNameAsync(sportDto.Name);
            if (sportItem != null)
            {
                _logger.LogInformation($"There is a sport exists with the name {sportDto.Name}");
                return BadRequest($"There is a sport exists with the name {sportDto.Name}");
            }

            var sport = new Sport
            {
                Name = sportDto.Name
            };

            await _sportRepository.AddAsync(sport);
            await _sportRepository.SaveChangesAsync();
            return CreatedAtAction(nameof(GetSportById), new { id = sport.SportId }, new SportCreationResponseDTO { SportId = sport.SportId});
        }

        // Update an existing sport
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateSport(int id, [FromBody] SportDTO sportDTO)
        {
            _logger.LogInformation($"Updating a sport by Name {sportDTO.Name}.");
            var sport = await _sportRepository.GetByIdAsync(id);
            if (sport == null)
            {
                _logger.LogWarning($"No sport is found with Id {id}.");
                return BadRequest($"There is a sport exists with id {id}");
            }
            sport.Name = sportDTO.Name;

            await _sportRepository.UpdateAsync(sport);
            await _sportRepository.SaveChangesAsync();
            return NoContent();
        }

        // Delete a sport by ID
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteSport(int id)
        {
            _logger.LogInformation($"Deleting a sport by Id {id}.");
            var sport = await _sportRepository.GetByIdAsync(id);
            if (sport == null)
            {
                _logger.LogWarning($"No sport is found with Id {id}.");
                return BadRequest($"There is a sport exists with id {id}");
            }

            await _sportRepository.DeleteAsync(id);
            await _sportRepository.SaveChangesAsync();
            return NoContent();
        }
    }
}