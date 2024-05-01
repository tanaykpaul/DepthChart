using AutoMapper;
using DC.Application.DTOs;
using DC.Application.Services;
using DC.Domain.Entities;
using DC.Domain.Logging;
using DC.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using System.Numerics;

namespace DC.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DepthChartController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAppLogger _logger;
        private readonly IMapper _mapper;

        public DepthChartController(IUnitOfWork unitOfWork, IAppLogger logger, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _mapper = mapper;
        }

        #region New Features
        // Add a player for a position into the depth chart (Use Case 1)
        [HttpPost("addFullDepthChart")]
        public async Task<ActionResult> AddFullDepthChart([FromBody] AddFullDepthChartDTO contents)
        {
            _logger.LogInformation($"Push a JSON object of Sport into the Depth Chart.");

            var service = new STP2FromJSON(_logger, _mapper);
            var sport = service.GetData(contents.JsonStringContents);
            var teamIds = new List<int>();

            // Add Sports Header part (Sport -> Teams -> Positions and Players
            if (sport.Item1 != null)
            {
                await _unitOfWork.SportRepository.AddAsync(sport.Item1);
                await _unitOfWork.SportRepository.SaveChangesAsync();
                teamIds.AddRange(sport.Item1.Teams.Select(x => x.TeamId));
            }
            else
            {
                _logger.LogWarning("JSON input is invalid at Sport Level.");
            }

            // Add Oders part into the Depth Chart
            if (sport.Item2 != null)
            {
                if(sport.Item2.Teams != null)
                {
                    int teamIdsIndex = 0;
                    foreach (var team in sport.Item2.Teams)
                    {
                        if(team != null)
                        {
                            if(team.Orders != null)
                            {
                                foreach(var orderDto in team.Orders)
                                {
                                    if(orderDto != null)
                                    {
                                        await _unitOfWork.AddPlayerToDepthChart(orderDto.PositionName, orderDto.PlayerNumber, orderDto.SeqNumber, teamIds[teamIdsIndex]);
                                    }
                                }
                            }                            
                        }
                    }
                    teamIdsIndex++;
                }               
            }
            else
            {
                _logger.LogWarning("JSON input is invalid at Order Level.");
            }

            return CreatedAtAction(nameof(GetFullDepthChart), "Full Depth Chart is created.");
        }
        #endregion

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