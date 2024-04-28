using DC.Application.DTOs;
using DC.Domain.Logging;
using DC.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

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

        #region Use Cases
        // Add a player for a position into the depth chart (Use Case 1)
        [HttpPost("addPlayerToDepthChart")]
        public async Task<ActionResult> AddPlayerToDepthChart([FromBody] AddPlayerToDepthChartDTO orderDto)
        {
            _logger.LogInformation($"Adding a player number {orderDto.PlayerNumber} for the position {orderDto.PositionName}");
            await _unitOfWork.AddPlayerToDepthChart(orderDto.PositionName, orderDto.PlayerNumber, orderDto.DepthPosition);

            return Created();
        }

        // Remove a player from the depth chart for a position (Use Case 2)
        [HttpDelete("removePlayerFromDepthChart")]
        public async Task<ActionResult<List<string>>> RemovePlayerFromDepthChart(string positionName, int playerNumber)
        {
            List<string> output = new List<string>();
            _logger.LogInformation($"Remove an order by the player number {playerNumber} for the position {positionName}");

            var players = await _unitOfWork.RemovePlayerFromDepthChart(positionName, playerNumber);
            if (players == null || players.Count == 0)
            {
                _logger.LogWarning($"There is no player number {playerNumber} in {positionName} position");
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

        // Get the Backup list of players for a position of a player
        [HttpGet("getBackups")]
        public async Task<ActionResult<List<string>>> GetBackUps(string positionName, int playerNumber)
        {
            List<string> output = new List<string>();
            _logger.LogInformation($"Find the Backups of the player number {playerNumber} for the position {positionName}");

            var players = await _unitOfWork.GetBackups(positionName, playerNumber);
            if (players == null || players.Count == 0)
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
                foreach (var item in list)
                {
                    string content = string.Empty;
                    foreach(var innerItem in item.Value)
                    {
                        content = content + $"(#{innerItem.Item1}, {innerItem.Item2}), ";
                    }
                    if(content.Length > 0)
                    {
                        content = content.Substring(0, content.Length - 2);
                    }
                    output.Add($"{item.Key} - {content}");
                }
            }

            return Ok(output);
        }
        #endregion
    }
}