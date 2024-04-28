using DC.Application.DTOs;
using DC.Domain.Entities;
using DC.Domain.Logging;
using DC.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace DC.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DepthChartController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAppLogger _logger;

        public DepthChartController(IUnitOfWork unitOfWork, IAppLogger logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        #region Sport Actions
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
        #endregion

        #region Team Actions
        // Get a specific team by ID
        [HttpGet("team/{id}")]
        public async Task<ActionResult<Team>> GetTeamById(int id)
        {
            var team = await _unitOfWork.TeamRepository.GetByIdAsync(id);
            if (team == null)
            {
                return NotFound();
            }
            return Ok(team);
        }        

        // Add a new team
        [HttpPost("team/add")]
        public async Task<ActionResult<TeamCreationResponseDTO>> AddTeam([FromBody] TeamDTO teamDto)
        {
            var teamItem = await _unitOfWork.TeamRepository.GetByTeamNameAndSportIdAsync(teamDto.Name, teamDto.SportId);
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

            await _unitOfWork.TeamRepository.AddAsync(team);
            await _unitOfWork.TeamRepository.SaveChangesAsync();
            return CreatedAtAction(nameof(GetTeamById), new { id = team.TeamId }, new TeamCreationResponseDTO { TeamId = team.TeamId, SportId = team.SportId });
        }
        #endregion

        #region Position Actions
        // Get a specific position by ID
        [HttpGet("position/{id}")]
        public async Task<ActionResult<Position>> GetPositionById(int id)
        {
            var position = await _unitOfWork.PositionRepository.GetByIdAsync(id);
            if (position == null)
            {
                return NotFound();
            }
            return Ok(position);
        }

        // Add a new position
        [HttpPost("position/add")]
        public async Task<ActionResult<PositionCreationResponseDTO>> AddPosition([FromBody] PositionDTO positionDto)
        {
            var positionItem = await _unitOfWork.PositionRepository.GetByPositionNameAndTeamIdAsync(positionDto.Name, positionDto.TeamId);
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

            await _unitOfWork.PositionRepository.AddAsync(position);
            await _unitOfWork.PositionRepository.SaveChangesAsync();
            return CreatedAtAction(nameof(GetPositionById), new { id = position.PositionId }, new PositionCreationResponseDTO { PositionId = position.PositionId, TeamId = position.TeamId });
        }
        #endregion

        #region Player Actions
        // Get a specific player by ID
        [HttpGet("player/{id}")]
        public async Task<ActionResult<Player>> GetPlayerById(int id)
        {
            var player = await _unitOfWork.PlayerRepository.GetByIdAsync(id);
            if (player == null)
            {
                return NotFound();
            }
            return Ok(player);
        }

        // Add a new player
        [HttpPost("player/add")]
        public async Task<ActionResult<PlayerCreationResponseDTO>> AddPlayer([FromBody] PlayerDTO playerDto)
        {
            var playerItem = await _unitOfWork.PlayerRepository.GetByPlayerNumberAndTeamIdAsync(playerDto.PlayerNumber, playerDto.TeamId);
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

            await _unitOfWork.PlayerRepository.AddAsync(player);
            await _unitOfWork.PlayerRepository.SaveChangesAsync();
            return CreatedAtAction(nameof(GetPlayerById), new { id = player.PlayerId }, new PlayerCreationResponseDTO { PlayerId = player.PlayerId, TeamId = player.TeamId });
        }
        #endregion

        #region Order Actions
        // Get a specific order by ID
        [HttpGet("details")]
        public async Task<ActionResult<Order>> GetOrderById(int positionId, int playerId)
        {
            _logger.LogInformation($"Fetching an order by positionId as {positionId} and playerId as {playerId}");
            var order = await _unitOfWork.OrderRepository.GetByIdAsync(positionId, playerId);
            if (order == null)
            {
                _logger.LogWarning($"There is no Order by positionId as {positionId} and playerId as {playerId}");
                return NotFound();
            }
            return Ok(order);
        }
        #endregion

        #region Use Cases
        // Add a player for a position into the depth chart (Use Case 1)
        [HttpPost("addPlayerToDepthChart")]
        public async Task<ActionResult<OrderCreationResponseDTO>> AddPlayerToDepthChart([FromBody] AddPlayerToDepthChartDTO orderDto)
        {
            await _unitOfWork.AddPlayerToDepthChart(orderDto.PositionName, orderDto.PlayerNumber, orderDto.DepthPosition);
            await _unitOfWork.SaveChangesAsync();
            return CreatedAtAction(nameof(GetOrderById), new { positionId = 1, playerId = 1 },
                new OrderCreationResponseDTO { PositionId = 1, PlayerId = 1 });
        }

        // Remove a player from the depth chart for a position (Use Case 2)
        [HttpDelete("removePlayerFromDepthChart")]
        public async Task<ActionResult<OrderCreationResponseDTO>> RemovePlayerFromDepthChart(string positionName, int playerNumber)
        {
            await _unitOfWork.RemovePlayerFromDepthChart(positionName, playerNumber);
            await _unitOfWork.SaveChangesAsync();
            return CreatedAtAction(nameof(GetOrderById), new { positionId = 1, playerId = 1 },
                new OrderCreationResponseDTO { PositionId = 1, PlayerId = 1 });
        }

        // Get the Backup list of players for a position of a player
        [HttpGet("getBackups")]
        public async Task<ActionResult<List<string>>> GetBackUps(string positionName, int playerNumber)
        {
            List<string> output = new List<string>();

            _logger.LogInformation($"Find the Backups of the player number {playerNumber} for the position {positionName}");
            var players = await _unitOfWork.GetBackups(positionName, playerNumber);
            if (players == null)
            {
                _logger.LogWarning($"There is no Backups for the player number {playerNumber} in {positionName} position");
                output.Add("<NO LIST>");
            }
            else
            {
                foreach (var item in players)
                {
                    output.Add($"#{item.Item1} – {item.Item2}");
                }
            }
            return Ok(output);
        }

        // Get the players for all positions of the team
        [HttpGet("getFullDepthChart")]
        public async Task<ActionResult<List<string>>> GetFullDepthChart()
        {
            List<string> output = new List<string>();
            _logger.LogInformation($"Find the players for all positions of the team");
            
            var list = await _unitOfWork.GetFullDepthChart();
            if (list == null)
            {
                _logger.LogWarning($"The Depth Chart has no positions");
                output.Add("<NO LIST>");
            }
            else
            {
                foreach(var item in list)
                {
                    output.Add($"{item.Key} - " + string.Join(',', $"(#{item.Value[0]}, {item.Value[1]})"));
                }
            }

            return Ok(output);
        }
        #endregion
    }
}