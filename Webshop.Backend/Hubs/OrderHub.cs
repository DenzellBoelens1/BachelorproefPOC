using Microsoft.AspNetCore.SignalR;
using Webshop.Backend.Services;
using Webshop.Shared.DTOs;

namespace Webshop.Backend.Hubs
{
    public class OrderHub : Hub
    {
        private readonly OrderService _orderService;

        public OrderHub(OrderService orderService)
        {
            _orderService = orderService;
        }

        public async Task GetOrdersByUser(int userId)
        {
            var orders = await _orderService.GetOrdersByUserAsync(userId);
            await Clients.Caller.SendAsync("ReceiveOrders", orders);
        }

        public async Task<OrderDTO.Created> PlaceOrder(OrderDTO.Create dto)
        {
            Console.WriteLine(">> PlaceOrder binnengekomen");
            var created = await _orderService.CreateOrderAsync(dto);
            // dit persist in de DB
            await Clients.Caller.SendAsync("OrderPlaced", created);
            return created;


        }
    }
}
