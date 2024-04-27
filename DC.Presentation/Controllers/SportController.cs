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
        private readonly IAppLogger<SportController> _logger;

        public SportController(ISportRepository sportRepository, IAppLogger<SportController> logger)
        {
            _sportRepository = sportRepository;
            _logger = logger;
        }

        // Get all sports
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Sport>>> GetAllSports()
        {
            var sports = await _sportRepository.GetAllAsync();
            return Ok(sports);
        }

        // Get a specific sport by ID
        [HttpGet("{id}")]
        public async Task<ActionResult<Sport>> GetSportById(int id)
        {
            var sport = await _sportRepository.GetByIdAsync(id);
            if (sport == null)
            {
                return NotFound();
            }
            return Ok(sport);
        }

        // Add a new sport
        [HttpPost]
        public async Task<ActionResult<SportCreationResponseDTO>> AddSport([FromBody] SportDTO sportDto)
        {
            var sportItem = await _sportRepository.GetByNameAsync(sportDto.Name);
            if (sportItem != null)
            {
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
        public async Task<ActionResult> UpdateSport(int id, [FromBody] Sport updatedSport)
        {
            if (id != updatedSport.SportId)
            {
                return BadRequest();
            }

            await _sportRepository.UpdateAsync(updatedSport);
            await _sportRepository.SaveChangesAsync();
            return NoContent();
        }

        // Delete a sport by ID
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteSport(int id)
        {
            await _sportRepository.DeleteAsync(id);
            await _sportRepository.SaveChangesAsync();
            return NoContent();
        }
    }
}