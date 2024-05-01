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
        [HttpGet("allPlayers")]
        public async Task<ActionResult<IEnumerable<PlayerCreationResponseDTO>>> GetAllPlayers()
        {
            _logger.LogInformation($"Finding all players...");
            var players = await _playerRepository.GetAllAsync();
            _logger.LogInformation($"There are {players.Count} players returned.");

            var playersDtoCollection = new List<PlayerCreationResponseDTO>();
            foreach (var player in players)
            {
                playersDtoCollection.Add(new PlayerCreationResponseDTO
                {
                    PlayerId = player.PlayerId,
                    PlayerName = player.Name,
                    PlayerNumber = player.Number,
                    Odds = player.Odds,
                    TeamId = player.TeamId
                });
            }
            return Ok(playersDtoCollection);
        }

        // Get a specific player by ID
        [HttpGet("{id}")]
        public async Task<ActionResult<PlayerCreationResponseDTO>> GetPlayerById(int id)
        {
            _logger.LogInformation($"Finding a player with Id {id}.");
            var player = await _playerRepository.GetByIdAsync(id);
            if (player == null)
            {
                _logger.LogWarning($"No player is found with Id {id}.");
                return NotFound($"No player is found with Id {id}.");
            }
            return Ok(new PlayerCreationResponseDTO
            {
                PlayerId = player.PlayerId,
                PlayerName = player.Name,
                PlayerNumber = player.Number,
                Odds = player.Odds,
                TeamId = player.TeamId
            });
        }

        // Add a new player
        [HttpPost("addPlayer")]
        public async Task<ActionResult<PlayerCreationResponseDTO>> AddPlayer([FromBody] PlayerDTO playerDto)
        {
            var playerItem = await _playerRepository.GetByPlayerNumberAndTeamIdAsync(playerDto.Number, playerDto.TeamId);
            if (!playerItem.Item2)
            {
                return BadRequest($"There is no team item is crreated yet by TeamId = {playerDto.TeamId}.");
            }
            if (playerItem.Item1 != null)
            {
                return BadRequest($"There is a player exists with the player number {playerDto.Number} under the team Id = {playerDto.TeamId}");
            }

            var player = new Player
            {
                Number = playerDto.Number,
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
        public async Task<ActionResult> UpdatePlayer(int id, [FromBody] PlayerDTO updatedPlayer)
        {
            _logger.LogInformation($"Updating a player by id {id}.");
            var player = await _playerRepository.GetByIdAsync(id);
            if (player == null)
            {
                _logger.LogWarning($"No player is found with Id {id}.");
                return BadRequest($"There is no player exists with id {id}");
            }

            player.Name = updatedPlayer.Name;
            player.Number = updatedPlayer.Number;
            player.Odds = updatedPlayer.Odds;

            await _playerRepository.UpdateAsync(player);
            await _playerRepository.SaveChangesAsync();
            return NoContent();
        }

        // Delete a player by ID
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeletePlayer(int id)
        {
            _logger.LogInformation($"Deleting a player by id {id}.");
            var player = await _playerRepository.GetByIdAsync(id);
            if (player == null)
            {
                _logger.LogWarning($"No player is found with Id {id}.");
                return BadRequest($"There is no player exists with id {id}");
            }

            await _playerRepository.DeleteAsync(id);
            await _playerRepository.SaveChangesAsync();
            return NoContent();
        }
    }
}