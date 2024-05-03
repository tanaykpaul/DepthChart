using DC.Domain.Entities;
using DC.Domain.Interfaces;
using DC.Domain.Logging;
using DC.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DC.Infrastructure.Services
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly DepthChartDbContext _dbContext;
        private readonly ISportRepository _sportRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IAppLogger _logger;
        private bool _disposed;

        public UnitOfWork(DepthChartDbContext dbContext, IAppLogger logger, ISportRepository sportRepository, IOrderRepository orderRepository)
        {
            _dbContext = dbContext;
            _logger = logger;
            _sportRepository = sportRepository;
            _orderRepository = orderRepository;
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _dbContext.SaveChangesAsync();
        }

        public async Task AddSportAsync(Sport sport)
        {
            await _sportRepository.AddAsync(sport);
        }

        /// <summary>
        /// Implementation of Use case 1
        /// </summary>
        /// <param name="positionName">A unique position name under a Team</param>
        /// <param name="playerNumber">A unique player number of a Team</param>
        /// <param name="depthPosition">Zero based order (first -> 0, second -> 1 ...) for the position of the player</param>
        /// <param name="teamId">If there is just one team in the db, teamId is optional</param>
        /// <returns></returns>
        public async Task AddPlayerToDepthChart(string positionName, int playerNumber, int? depthPosition, int teamId = 1)
        {
            // Checking the team entry was completed before using this use case
            var team = await _dbContext.Teams.FindAsync(teamId);
            if (team != null)
            {
                var positionIdAndPlayerId = await GetPositionIdAndPlayerId(positionName, playerNumber, teamId);
                if(positionIdAndPlayerId.Item1 != null && positionIdAndPlayerId.Item2 != null)
                {
                    await _orderRepository.AddPlayerToDepthChart(positionIdAndPlayerId.Item1.Value, positionIdAndPlayerId.Item2.Value, depthPosition);
                    await _orderRepository.SaveChangesAsync();                    
                }
            }
        }

        /// <summary>
        /// Implementation of Use case 2
        /// </summary>
        /// <param name="positionName">A unique position name under a Team</param>
        /// <param name="playerNumber">A unique player number of a Team</param>
        /// <param name="teamId">If there is just one team in the db, teamId is optional</param>
        /// <returns>Either an empty List or the Player that is removed</returns>
        public async Task<List<(int, string)>> RemovePlayerFromDepthChart(string positionName, int playerNumber, int teamId = 1)
        {
            List<(int, string)> output = [];

            // Checking the team entry was completed before using this use case
            var team = await _dbContext.Teams.FindAsync(teamId);
            if (team != null)
            {
                var positionIdAndPlayerId = await GetPositionIdAndPlayerId(positionName, playerNumber, teamId);
                if (positionIdAndPlayerId.Item1 != null && positionIdAndPlayerId.Item2 != null)
                {
                    bool isRemoved = await _orderRepository.RemovePlayerFromDepthChart(positionIdAndPlayerId.Item1.Value, positionIdAndPlayerId.Item2.Value);
                    await _orderRepository.SaveChangesAsync();
                    if(isRemoved)
                    {
                        var player = await _dbContext.Players.FindAsync(positionIdAndPlayerId.Item2.Value);
                        if(player != null)
                            output.Add((player.Number, player.Name));
                    }
                }
            }
            return output;
        }

        /// <summary>
        /// Implementation of Use case 3
        /// </summary>
        /// <param name="positionId">Index of the position entry for a team</param>
        /// <param name="playerId">Index of the player entry for a team</param>
        /// <param name="teamId">If there is just one team in the db, teamId is optional</param>
        /// <returns>Either an empty List or List of Players those are Backups</returns>
        public async Task<List<(int, string)>> GetBackups(string positionName, int playerNumber, int teamId = 1)
        {
            List<(int, string)> output = [];

            // Checking the team entry was completed before using this use case
            var team = await _dbContext.Teams.FindAsync(teamId);
            if (team != null)
            {
                var positionIdAndPlayerId = await GetPositionIdAndPlayerId(positionName, playerNumber, teamId);
                if (positionIdAndPlayerId.Item1 != null && positionIdAndPlayerId.Item2 != null)
                {
                    var order = await _orderRepository.GetByIdAsync(positionIdAndPlayerId.Item1.Value, positionIdAndPlayerId.Item2.Value);
                    if (order != null)
                    {
                        // Business Rule 1: For a given player and position, return all players that are “Backups”, those with a lower position_depth
                        var orders = await _dbContext.Orders
                                                        .Where(x => x.PositionId == positionIdAndPlayerId.Item1.Value &&
                                                                    x.SeqNumber > order.SeqNumber)
                                                        .OrderBy(x => x.SeqNumber)
                                                        .Include(x => x.Player)
                                                        .ToListAsync();

                        foreach(var orderObj in orders)
                        {
                            output.Add((orderObj.Player.Number, orderObj.Player.Name));
                        }

                        // Business Rule 2: An empty list should be returned if the given player has no Backups
                        // For the use case, no action is required here, final return statement is enough
                    }

                    // Business Rule 3: An empty list should be returned, if the given player is not listed in the depth chart at that position                    
                    // For the use case, no action is required here, final return statement is enough
                }
            }
            return output;
        }

        /// <summary>
        /// Find the players for all positions of a team
        /// </summary>
        /// <param name="teamId">If there is just one team in the db, teamId is optional</param>
        /// <returns>Key of the dictionary is the position name, First item of each List tuple is the player number and the other one is the player name</returns>
        public async Task<IDictionary<string, List<(int, string)>>> GetFullDepthChart(int teamId = 1)
        {
            IDictionary<string, List<(int, string)>> output = new Dictionary<string, List<(int, string)>>();
            
            // Checking the team entry was completed before using this use case
            var team = await _dbContext.Teams.FindAsync(teamId);
            if (team != null)
            {
                var positions = await _dbContext.Positions
                                            .Where(x => x.TeamId == teamId)
                                            .Include(x => x.Orders)
                                            .ThenInclude(y => y.Player)
                                            .ToListAsync();

                foreach (var position in positions)
                {
                    var orders = position.Orders.OrderBy(x => x.SeqNumber);
                    List<(int playerNumber, string playerName)> playersList = [];
                    foreach (var order in orders)
                    {
                        int playerNumber = order.Player.Number;
                        string playerName = order.Player.Name;

                        playersList.Add((playerNumber, playerName));
                    }
                    output.Add(position.Name, playersList);
                }
            }            
            return output;
        }


        /// <summary>
        /// Get the primary key values of position and player entities
        /// </summary>
        /// <param name="positionId">Index of the position entry for a team</param>
        /// <param name="playerId">Index of the player entry for a team</param>
        /// <param name="teamId">Index of the team entry</param>
        /// <returns>null or Index of the position and player entries respectively</returns>
        private async Task<(int?, int?)> GetPositionIdAndPlayerId(string positionName, int playerNumber, int teamId)
        {
            // Find positionId by the given positionName under the teamId
            var position = await _dbContext.Positions
                .Where(x => x.Name == positionName && x.TeamId == teamId)
                .FirstOrDefaultAsync();
            
            // Find playerId by the given playerNumber under the teamId
            var player = await _dbContext.Players
                .Where(x => x.Number == playerNumber && x.TeamId == teamId)
                .FirstOrDefaultAsync();

            return (position?.PositionId, player?.PlayerId);
        }
    }
}