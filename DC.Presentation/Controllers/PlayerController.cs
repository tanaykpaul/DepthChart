using DC.Application.DTOs;
using DC.Domain.Entities;
using DC.Domain.Interfaces;
using DC.Domain.Logging;
using Microsoft.AspNetCore.Mvc;

namespace DC.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PlayerController : ControllerBase
    {
        private readonly IPlayerRepository _playerRepository;
        private readonly IAppLogger _logger;

        public PlayerController(IPlayerRepository playerRepository, IAppLogger logger)
        {
            _playerRepository = playerRepository;
            _logger = logger;
        }

        // Get all players
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Player>>> GetAllPlayers()
        {
            var players = await _playerRepository.GetAllAsync();
            return Ok(players);
        }

        // Get a specific player by ID
        [HttpGet("{id}")]
        public async Task<ActionResult<Player>> GetPlayerById(int id)
        {
            var player = await _playerRepository.GetByIdAsync(id);
            if (player == null)
            {
                return NotFound();
            }
            return Ok(player);
        }

        // Add a new player
        [HttpPost]
        public async Task<ActionResult<PlayerCreationResponseDTO>> AddPlayer([FromBody] PlayerDTO playerDto)
        {
            var playerItem = await _playerRepository.GetByPlayerNumberAndTeamIdAsync(playerDto.PlayerNumber, playerDto.TeamId);
            if (!playerItem.Item2)
            {
                return BadRequest($"There is no team item is crreated yet by TeamId = {playerDto.TeamId}.");
            }
            if (playerItem.Item1 != null)
            {
                return BadRequest($"There is a player exists with the player number {playerDto.PlayerNumber} under the team Id = {playerDto.TeamId}");
            }

            var player = new Player
            {
                Number = playerDto.PlayerNumber,
                Name = playerDto.Name,
                Odds = playerDto.Odds,
                TeamId = playerDto.TeamId
            };

            await _playerRepository.AddAsync(player);
            await _playerRepository.SaveChangesAsync();
            return CreatedAtAction(nameof(GetPlayerById), new { id = player.PlayerId }, new PlayerCreationResponseDTO { PlayerId = player.PlayerId, TeamId = player.TeamId });
        }

        // Update an existing player
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdatePlayer(int id, [FromBody] Player updatedPlayer)
        {
            if (id != updatedPlayer.PlayerId)
            {
                return BadRequest();
            }

            var existingPlayer = await _playerRepository.GetByIdAsync(id);
            if (existingPlayer == null)
            {
                return NotFound();
            }

            await _playerRepository.UpdateAsync(updatedPlayer);
            await _playerRepository.SaveChangesAsync();
            return NoContent();
        }

        // Delete a player by ID
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeletePlayer(int id)
        {
            var existingPlayer = await _playerRepository.GetByIdAsync(id);
            if (existingPlayer == null)
            {
                return NotFound();
            }

            await _playerRepository.DeleteAsync(id);
            await _playerRepository.SaveChangesAsync();
            return NoContent();
        }
    }
}