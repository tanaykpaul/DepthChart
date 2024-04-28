using DC.Domain.Entities;
using DC.Domain.Interfaces;
using DC.Domain.Logging;
using DC.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DC.Infrastructure.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly DepthChartDbContext _context;
        private readonly IAppLogger _logger;

        public OrderRepository(DepthChartDbContext context, IAppLogger logger)
        {
            _context = context;
            _logger = logger;
        }

        // Get a specific Order by PositionId and PlayerId
        public async Task<Order?> GetByIdAsync(int positionId, int playerId)
        {
            _logger.LogInformation($"Retrieving order with PositionId as {positionId} and PlayerId as {playerId}");
            return await _context.Orders
                                .Include(o => o.Player)
                                .Include(o => o.Position)
                                .FirstOrDefaultAsync(o => o.PositionId == positionId && o.PlayerId == playerId);
        }

        // Get all Orders
        public async Task<List<Order>> GetAllAsync()
        {
            _logger.LogInformation("Retrieving all orders");
            return await _context.Orders
                                 .OrderBy(x => x.PositionId)
                                 .ThenBy(x => x.SeqNumber)
                                 .Include(o => o.Player)
                                 .Include(o => o.Position)
                                 .ToListAsync();
        }

        // Add a new Order
        public async Task AddAsync(Order order)
        {
            _logger.LogInformation("Adding a new order");
            
            // Check if the order with the same composite key already exists
            var existingOrder = await _context.Orders
                .FirstOrDefaultAsync(o => o.PositionId == order.PositionId && o.PlayerId == order.PlayerId);

            if (existingOrder != null)
            {
                // Handle the conflict (e.g., log a message, return an error, update the existing order, etc.)
                throw new InvalidOperationException("An order with the same PositionId and PlayerId already exists.");
            }

            // If no existing order found, add the new order
            await _context.Orders.AddAsync(order);            
        }

        public async Task UpdateAsync(Order order)
        {
            _logger.LogInformation($"Updating order with PositionId as {order.PositionId} and PlayerId as {order.PlayerId}");
            _context.Orders.Update(order);
        }

        // Save changes to the database
        public async Task SaveChangesAsync()
        {
            _logger.LogInformation("Saving changes to the database");
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Ideal implementation of Use case 1
        /// </summary>
        /// <param name="positionId">Index of the position entry for a team</param>
        /// <param name="playerId">Index of the player entry for a team</param>
        /// <param name="depthPosition">Zero based order (first -> 0, second -> 1 ...) 
        /// for the position of the player</param>
        /// <returns></returns>
        public async Task AddPlayerToDepthChart(int positionId, int playerId, int? depthPosition)
        {
            // Business Rule 1: If the depthPosition is null,
            // Set depthPosition as the next to the max value in the depth chart
            if (depthPosition == null)
            {
                // Get the next sequence number 
                var orders = _context.Orders.Where(x => x.PositionId == positionId);
                if (orders != null && orders.Any())
                {
                    depthPosition = orders.Max(x => x.SeqNumber) + 1; // Last place in the position
                }
                else
                {
                    depthPosition = 0; // First place in the position
                }                   
            }

            // Create a new Order item
            var order = new Order
            {
                PositionId = positionId,
                PlayerId = playerId,
                SeqNumber = depthPosition.Value
            };

            // Business Rule 2: If depthPosition is occupied by another player,
            // The adding player will get the priority
            // The other players from that depthPosition will move down one place
            var orderFromDepthPosition = _context.Orders.Where(x => x.PositionId == positionId && x.SeqNumber >= depthPosition.Value);
            if(orderFromDepthPosition != null && await orderFromDepthPosition.FirstOrDefaultAsync(x => x.SeqNumber == depthPosition) != null)
            {
                // Move down the remaining players from the depthPosition
                await orderFromDepthPosition.ForEachAsync(x => ++x.SeqNumber);
            }
            
            await AddAsync(order);
        }

        // <summary>
        /// Ideal implementation of Use case 2
        /// </summary>
        /// <param name="positionId">Index of the position entry for a team</param>
        /// <param name="playerId">Index of the player entry for a team</param>
        /// <returns>Removed the player or not</returns>
        public async Task<bool> RemovePlayerFromDepthChart(int positionId, int playerId)
        {
            _logger.LogInformation($"Deleting order with PositionId as {positionId} and PlayerId as {playerId}");
            var order =  await _context.Orders.Where(x => x.PositionId == positionId && x.PlayerId == playerId).FirstOrDefaultAsync();
            if (order != null)
            {
                _context.Orders.Remove(order);
                return true;
            }
            return false;
        }        
    }
}