using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Webshop.Backend.Data;
using Webshop.Backend.Services;
using Webshop.Shared.DTOs;

namespace Webshop.Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly OrderService _orderService;

        public OrdersController(OrderService orderService)
        {
            _orderService = orderService;
        }


        [HttpGet("user/{userId}")]
        public async Task<ActionResult<List<OrderDTO.Index>>> GetOrdersByUser(int userId)
        {
            var orders = await _orderService.GetOrdersByUserAsync(userId);
            return Ok(orders);
        }

        [HttpPost]
        public async Task<ActionResult<OrderDTO.Created>> CreateOrder([FromBody] OrderDTO.Create dto)
        {
            var created = await _orderService.CreateOrderAsync(dto);
            return Ok(created);
        }

    }

}
