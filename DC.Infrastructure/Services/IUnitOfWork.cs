using DC.Domain.Interfaces;

namespace DC.Infrastructure.Services
{
    public interface IUnitOfWork : IDisposable
    {
        ISportRepository SportRepository { get; }
        ITeamRepository TeamRepository { get; }
        IPositionRepository PositionRepository { get; }
        IPlayerRepository PlayerRepository { get; }
        IOrderRepository OrderRepository { get; }

        Task<int> SaveChangesAsync();
    }
}