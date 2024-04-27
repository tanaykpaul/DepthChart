﻿using DC.Domain.Entities;
using DC.Domain.Interfaces;
using DC.Domain.Logging;
using DC.Infrastructure.Data;
using DC.Infrastructure.Services;
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
                                 .Include(o => o.Player)
                                 .Include(o => o.Position)
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
            _logger.LogInformation($"Updating order with PositionId as {order.PositionId} and PlayerId as {order.PlayerId}");
            _context.Orders.Update(order);
        }

        // Delete an Order by PositionId and PlayerId
        public void Delete(int positionId, int playerId)
        {
            _logger.LogInformation($"Deleting order with PositionId as {positionId} and PlayerId as {playerId}");
            var order = _context.Orders.Find(positionId, playerId);
            if (order != null)
            {
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
        /// Implementation of Use case 1 (Not ideal to use it, rather use the other AddPlayerToDepthChart method)
        /// </summary>
        /// <param name="positionName">A unique position name under a Team</param>
        /// <param name="playerNumber">A unique player number of a Team</param>
        /// <param name="depthPosition">Zero based order (first -> 0, second -> 1 ...) 
        /// for the position of the player</param>
        /// <param name="teamId">If there is just one team in the db, teamId is optional</param>
        /// <returns></returns>
        public async Task<Order?> AddPlayerToDepthChart(string positionName, int playerNumber, int? depthPosition, int teamId = 1)
        {
            // Checking the team entry was completed before using this use case
            var team = await _context.Teams.FindAsync(teamId);
            if (team != null)
            {
                // Find positionId by the given positionName
                var position = await _context.Positions
                    .Where(x => x.Name == positionName && x.TeamId == teamId)
                    .FirstOrDefaultAsync();

                if(position != null)
                {
                    // Find playerId by the given playerNumber
                    var player = await _context.Players
                        .Where(x => x.Number == playerNumber && x.TeamId == teamId)
                        .FirstOrDefaultAsync();

                    if(player != null)
                    {
                        await AddPlayerToDepthChart(position.PositionId, player.PlayerId, teamId);
                    }                    
                }
            }

            return null;
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
                if (orders != null)
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
    }
}