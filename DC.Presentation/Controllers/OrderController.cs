using DC.Application.DTOs;
using DC.Domain.Entities;
using DC.Domain.Interfaces;
using DC.Domain.Logging;
using Microsoft.AspNetCore.Mvc;

namespace DC.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IAppLogger _logger;

        public OrderController(IOrderRepository orderRepository, IAppLogger logger)
        {
            _orderRepository = orderRepository;
            _logger = logger;
        }

        // Get all orders
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Order>>> GetAllOrders()
        {
            _logger.LogInformation("Fetching all orders");
            var orders = await _orderRepository.GetAllAsync();
            return Ok(orders);
        }

        // Get a specific order by ID
        [HttpGet("details")]
        public async Task<ActionResult<Order>> GetOrderById(int positionId, int playerId)
        {
            _logger.LogInformation($"Fetching an order by positionId as {positionId} and playerId as {playerId}");
            var order = await _orderRepository.GetByIdAsync(positionId, playerId);
            if (order == null)
            {
                _logger.LogWarning($"There is no Order by positionId as {positionId} and playerId as {playerId}");
                return NotFound();
            }
            return Ok(order);
        }

        // Add a new order
        [HttpPost]
        public async Task<ActionResult<OrderCreationResponseDTO>> AddOrderUseCase1([FromBody] OrderDTO orderDto)
        {
            await _orderRepository.AddPlayerToDepthChart(orderDto.PositionId, orderDto.PlayerId, orderDto.SeqNumber);
            await _orderRepository.SaveChangesAsync();
            return CreatedAtAction(nameof(GetOrderById), new { positionId = orderDto.PositionId, playerId = orderDto.PlayerId }, 
                new OrderCreationResponseDTO { PositionId = orderDto.PositionId, PlayerId = orderDto.PlayerId });
        }

        // Update an existing order
        [HttpPut("details")]
        public async Task<ActionResult> UpdateOrder(int positionId, int playerId, [FromBody] Order updatedOrder)
        {
            if (positionId != updatedOrder.PositionId && playerId != updatedOrder.PlayerId)
            {
                return BadRequest();
            }

            var existingOrder = await _orderRepository.GetByIdAsync(positionId, playerId);
            if (existingOrder == null)
            {
                return NotFound();
            }

            await _orderRepository.UpdateAsync(updatedOrder);
            await _orderRepository.SaveChangesAsync();
            return NoContent();
        }

        // Delete an order by ID
        [HttpDelete("details")]
        public async Task<ActionResult> DeleteOrder(int positionId, int playerId)
        {
            var existingOrder = await _orderRepository.GetByIdAsync(positionId, playerId);
            if (existingOrder == null)
            {
                return NotFound();
            }

            _orderRepository.Delete(positionId, playerId);
            await _orderRepository.SaveChangesAsync();
            return NoContent();
        }
    }
}