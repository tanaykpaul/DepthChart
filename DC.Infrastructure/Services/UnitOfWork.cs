using DC.Domain.Entities;
using DC.Domain.Interfaces;
using DC.Domain.Logging;
using DC.Infrastructure.Data;
using DC.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DC.Infrastructure.Services
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly DepthChartDbContext _dbContext;
        private readonly IAppLogger _logger;
        private bool _disposed;

        public UnitOfWork(DepthChartDbContext dbContext, IAppLogger logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        // Lazy initialization of repositories
        private ISportRepository _sportRepository;
        private ITeamRepository _teamRepository;
        private IPositionRepository _positionRepository;
        private IPlayerRepository _playerRepository;
        private IOrderRepository _orderRepository;

        public ISportRepository SportRepository => _sportRepository ??= new SportRepository(_dbContext, _logger);
        public ITeamRepository TeamRepository => _teamRepository ??= new TeamRepository(_dbContext, _logger);
        public IPositionRepository PositionRepository => _positionRepository ??= new PositionRepository(_dbContext, _logger);
        public IPlayerRepository PlayerRepository => _playerRepository ??= new PlayerRepository(_dbContext, _logger);
        public IOrderRepository OrderRepository => _orderRepository ??= new OrderRepository(_dbContext, _logger);

        public async Task<int> SaveChangesAsync()
        {
            return await _dbContext.SaveChangesAsync();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _dbContext?.Dispose();
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Implementation of Use case 1
        /// </summary>
        /// <param name="positionName">A unique position name under a Team</param>
        /// <param name="playerNumber">A unique player number of a Team</param>
        /// <param name="depthPosition">Zero based order (first -> 0, second -> 1 ...) 
        /// for the position of the player</param>
        /// <param name="teamId">If there is just one team in the db, teamId is optional</param>
        /// <returns></returns>
        public async Task<Order?> AddPlayerToDepthChart(string positionName, int playerNumber, int? depthPosition, int teamId = 1)
        {
            await HandleAddOrRemoveOrder(positionName, playerNumber, true, teamId);
            return null;
        }

        /// <summary>
        /// Implementation of Use case 2
        /// </summary>
        /// <param name="positionName">A unique position name under a Team</param>
        /// <param name="playerNumber">A unique player number of a Team</param>
        /// <returns></returns>
        public async Task<Player?> RemovePlayerFromDepthChart(string positionName, int playerNumber, int teamId = 1)
        {
            await HandleAddOrRemoveOrder(positionName, playerNumber, false, teamId);
            return null;
        }

        /// <summary>
        /// Implementation of Use case 3
        /// </summary>
        /// <param name="positionId">Index of the position entry for a team</param>
        /// <param name="playerId">Index of the player entry for a team</param>
        /// <returns>Either Empty List or List of Players those are Backups</returns>
        public async Task<ICollection<Player>> GetBackups(int positionId, int playerId)
        {
            ICollection<Player> output = new List<Player>();

            var order = await _orderRepository.GetByIdAsync(positionId, playerId);
            if(order != null)
            {
                // Business Rule 1: For a given player and position,
                // Return all players that are “Backups”, those with a lower position_depth
                var list = await _dbContext.Orders
                                        .Where(x => x.PositionId == positionId &&
                                            x.PlayerId == playerId &&
                                            x.SeqNumber > order.SeqNumber).ToListAsync();

                // find the list of players
            }

            // Business Rule 3: An empty list should be returned
            // if the given player is not listed in the depth chart at that position

            return output;
        }

        private async Task HandleAddOrRemoveOrder(string positionName, int playerNumber, bool isAdd, int teamId = 1)
        {
            // Checking the team entry was completed before using this use case
            var team = await _dbContext.Teams.FindAsync(teamId);
            if (team != null)
            {
                // Find positionId by the given positionName
                var position = await _dbContext.Positions
                    .Where(x => x.Name == positionName && x.TeamId == teamId)
                    .FirstOrDefaultAsync();

                if (position != null)
                {
                    // Find playerId by the given playerNumber
                    var player = await _dbContext.Players
                        .Where(x => x.Number == playerNumber && x.TeamId == teamId)
                        .FirstOrDefaultAsync();

                    if (player != null)
                    {
                        if (isAdd)
                        {
                            await _orderRepository.AddPlayerToDepthChart(position.PositionId, player.PlayerId, teamId);
                        }
                        else
                        {
                            _orderRepository.RemovePlayerFromDepthChart(position.PositionId, player.PlayerId);
                        }
                    }
                }
            }
        }
    }
}