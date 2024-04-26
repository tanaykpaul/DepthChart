using DC.Domain.Entities;
using DC.Domain.Interfaces;
using DC.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DC.Infrastructure.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly DepthChartDbContext _context;
        private readonly ILogger<OrderRepository> _logger;

        public OrderRepository(DepthChartDbContext context, ILogger<OrderRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Get a specific Order by ID
        public async Task<Order?> GetByIdAsync(int id)
        {
            _logger.LogInformation($"Retrieving order with ID: {id}");
            return await _context.Orders
                                 .Include(o => o.Player) // Include the Player associated with the Order
                                 .Include(o => o.Position) // Include the Position associated with the Order
                                 .FirstOrDefaultAsync(o => o.OrderId == id);
        }

        // Get all Orders
        public async Task<List<Order>> GetAllAsync()
        {
            _logger.LogInformation("Retrieving all orders");
            return await _context.Orders
                                 .Include(o => o.Player) // Include the Players associated with the Orders
                                 .Include(o => o.Position) // Include the Positions associated with the Orders
                                 .ToListAsync();
        }

        // Add a new Order
        public async Task AddAsync(Order order)
        {
            _logger.LogInformation("Adding a new order");
            await _context.Orders.AddAsync(order);
        }

        // Update an existing Order
        public async Task UpdateAsync(Order order)
        {
            _logger.LogInformation($"Updating order with ID: {order.OrderId}");
            _context.Orders.Update(order);
        }

        // Delete an Order by ID
        public async Task DeleteAsync(int id)
        {
            _logger.LogInformation($"Deleting order with ID: {id}");
            var order = await _context.Orders.FindAsync(id);
            if (order != null)
            {
                _logger.LogWarning($"Order with ID: {id} not found for deletion");
                _context.Orders.Remove(order);
            }
        }

        // Save changes to the database
        public async Task SaveChangesAsync()
        {
            _logger.LogInformation("Saving changes to the database");
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Use case 1: 
        /// </summary>
        /// <param name="positionId"></param>
        /// <param name="playerId"></param>
        /// <param name="depthPosition"></param>
        /// <returns></returns>
        public async Task<int?> AddPlayerToDepthChart(int positionId, int playerId, int? depthPosition)
        {
            int? newOrderId = null;
            // Check positionId and playerId are valid
            if (_context.Positions.FindAsync(positionId).Result != null && _context.Players.FindAsync(playerId).Result != null)
            {
                // Business Rule 1: If the depthPosition is null,
                // Set depthPosition as the next to the max value in the depth chart
                if (depthPosition == null)
                {
                    // Get the next sequence number 
                    var orders = _context.Orders.Where(x => x.PositionId == positionId);
                    if (orders != null)
                    {
                        depthPosition = orders.Max(x => x.SeqNumber) + 1;
                    }
                    else
                    {
                        depthPosition = 1;
                    }                   
                }

                // Create a new Order item
                var order = new Order();
                order.PositionId = positionId;
                order.PlayerId = playerId;
                order.SeqNumber = depthPosition.Value;

                // Business Rule 2: If depthPosition is occupied by another player,
                // The adding player will get the priority                
                var orderFromDepthPosition = _context.Orders.Where(x => x.PositionId == positionId && x.SeqNumber >= depthPosition.Value);
                if(orderFromDepthPosition != null && await orderFromDepthPosition.FirstOrDefaultAsync(x => x.SeqNumber == depthPosition) != null)
                {
                    // Move down the remaining players from the depthPosition
                    await orderFromDepthPosition.ForEachAsync(x => x.SeqNumber = x.SeqNumber + 1);
                }

                await AddAsync(order);
                newOrderId = order.OrderId;
            }

            return newOrderId;
        }

        /// <summary>
        /// Use case 2: Remove 
        /// </summary>
        /// <param name="positionId"></param>
        /// <param name="playerId"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public Task<Player>? RemovePlayerToDepthChart(int positionId, int playerId)
        {
            throw new NotImplementedException();
        }

        public Task<Player>? GetBackups(int positionId, int playerId)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="playerId"></param>
        /// <param name="positionId"></param>
        /// <returns></returns>
        public async Task<(Order?, bool)> GetByPlayerIdAndPositionIdAsync(int playerId, int positionId)
        {
            if (_context.Positions.FindAsync(positionId).Result != null && _context.Players.FindAsync(playerId).Result != null)
            {
                return (await _context.Orders.Where(x => x.PlayerId == playerId && x.PositionId == positionId).FirstOrDefaultAsync(), true);
            }

            return (null, false);
        }
    }
}