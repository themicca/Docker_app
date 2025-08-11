using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Order.Model;
using Order.Service;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrderService.WebAPI.Controllers
{
    [ApiController]
    [Route("orders")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Get([FromQuery] string? status)
        {
            if (string.IsNullOrWhiteSpace(status))
            {
                var all = await _orderService.GetOrdersAsync();
                return Ok(all);
            }

            var filtered = await _orderService.GetOrdersByStatusAsync(status);
            return Ok(filtered);
        }

        [HttpGet("{orderId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetOrderById(Guid orderId)
        {
            var order = await _orderService.GetOrderByIdAsync(orderId);
            if (order != null)
            {
                return Ok(order);
            }
            else
            {
                return NotFound();
            }
        }

        [HttpPut("{orderId}/status")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateStatus(Guid orderId, [FromBody] string status)
        {
            if (status is null || string.IsNullOrWhiteSpace(status))
                return BadRequest("NewStatus is required.");

            await _orderService.UpdateOrderStatusAsync(orderId, status);

            return Ok();
        }

        [HttpPost]
        [ProducesResponseType(typeof(OrderDetail), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateOrderRequest request)
        {
            var result = await _orderService.CreateOrderAsync(request);
            if (result.Order == null) return BadRequest(result.Error);

            return CreatedAtAction(
                nameof(GetOrderById),
                new { orderId = result.Order.Id },
                result.Order
            );
        }

        [HttpGet("profit-by-month")]
        [ProducesResponseType(typeof(IEnumerable<MonthlyProfitRequest>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ProfitByMonth()
        {
            var data = await _orderService.GetMonthlyProfitAsync();
            return Ok(data);
        }

        [HttpGet("products")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetProducts()
        {
            var products = await _orderService.GetProductsAsync();
            return Ok(products);
        }
    }
}
