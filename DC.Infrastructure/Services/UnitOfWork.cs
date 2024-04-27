using DC.Domain.Interfaces;
using DC.Domain.Logging;
using DC.Infrastructure.Data;
using DC.Infrastructure.Repositories;

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
    }
}