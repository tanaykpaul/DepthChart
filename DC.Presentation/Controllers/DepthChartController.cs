using DC.Application.DTOs;
using DC.Domain.Entities;
using DC.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace DC.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DepthChartController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public DepthChartController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllTeams()
        {
            var teams = await _unitOfWork.TeamRepository.GetAllAsync();
            return Ok(teams);
        }

        // Get a specific sport by ID
        [HttpGet("sport/{id}")]
        public async Task<ActionResult<Sport>> GetSportById(int id)
        {
            var sport = await _unitOfWork.SportRepository.GetByIdAsync(id);
            if (sport == null)
            {
                return NotFound();
            }
            return Ok(sport);
        }

        [HttpPost("sport/add")]
        public async Task<ActionResult<SportCreationResponseDTO>> AddSport([FromBody] SportDTO sportDto)
        {
            var sportItem = await _unitOfWork.SportRepository.GetByNameAsync(sportDto.Name);
            if (sportItem != null)
            {
                return BadRequest($"There is a sport exists with the name {sportDto.Name}");
            }

            var sport = new Sport
            {
                Name = sportDto.Name
            };

            await _unitOfWork.SportRepository.AddAsync(sport);
            await _unitOfWork.SportRepository.SaveChangesAsync();
            return CreatedAtAction(nameof(GetSportById), new { id = sport.SportId }, new SportCreationResponseDTO { SportId = sport.SportId });
        }
    }
}