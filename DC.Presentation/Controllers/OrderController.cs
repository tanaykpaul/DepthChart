using DC.Application.DTOs;
using DC.Domain.Entities;
using DC.Domain.Interfaces;
using DC.Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace DC.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderRepository _orderRepository;
        private readonly Domain.Interfaces.ILogger<OrderController> _logger;

        public OrderController(IOrderRepository orderRepository, Domain.Interfaces.ILogger<OrderController> logger)
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
        [HttpGet("{id}")]
        public async Task<ActionResult<Order>> GetOrderById(int id)
        {
            _logger.LogInformation("Fetching an order by id {id}");
            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null)
            {
                _logger.LogWarning($"There is no Orders by id {id}");
                return NotFound();
            }
            return Ok(order);
        }

        // Add a new order
        [HttpPost]
        public async Task<ActionResult<OrderCreationResponseDTO>> AddOrder([FromBody] OrderDTO orderDto)
        {
            var orderItem = await _orderRepository.GetByPlayerIdAndPositionIdAsync(orderDto.PlayerId, orderDto.PositionId);
            if (!orderItem.Item2)
            {
                var message = $"Either playerId as {orderDto.PlayerId} or positionId as {orderDto.PositionId} is not present in the corresponding table.";
                _logger.LogError(message);
                return BadRequest(message);
            }
            if (orderItem.Item1 != null)
            {
                var message = $"There is an order exists with the playerId as {orderDto.PlayerId} and positionId as {orderDto.PositionId}";
                _logger.LogError(message);
                return BadRequest(message);
            }

            return await AddOrderUseCase1(orderDto);
        }

        // Add a new order for Use case 1
        [HttpPost("UseCase1")]
        public async Task<ActionResult<OrderCreationResponseDTO>> AddOrderUseCase1([FromBody] OrderDTO orderDto)
        {
            var newOrderId = await _orderRepository.AddPlayerToDepthChart(orderDto.PositionId, orderDto.PlayerId, orderDto.SeqNumber);
            if(newOrderId == null)
            {
                return BadRequest($"Order is not created. Check your PositionId and PlayerId");
            }

            await _orderRepository.SaveChangesAsync();
            return CreatedAtAction(nameof(GetOrderById), new { id = newOrderId }, 
                new OrderCreationResponseDTO { OrderId = newOrderId.Value, PositionId = orderDto.PositionId, PlayerId = orderDto.PlayerId });
        }

        // Update an existing order
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateOrder(int id, [FromBody] Order updatedOrder)
        {
            if (id != updatedOrder.OrderId)
            {
                return BadRequest();
            }

            var existingOrder = await _orderRepository.GetByIdAsync(id);
            if (existingOrder == null)
            {
                return NotFound();
            }

            await _orderRepository.UpdateAsync(updatedOrder);
            await _orderRepository.SaveChangesAsync();
            return NoContent();
        }

        // Delete an order by ID
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteOrder(int id)
        {
            var existingOrder = await _orderRepository.GetByIdAsync(id);
            if (existingOrder == null)
            {
                return NotFound();
            }

            await _orderRepository.DeleteAsync(id);
            await _orderRepository.SaveChangesAsync();
            return NoContent();
        }
    }
}