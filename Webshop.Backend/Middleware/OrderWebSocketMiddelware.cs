using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Webshop.Backend.Services;
using Webshop.Shared.DTOs;

namespace Webshop.Backend.Middleware
{
    public class OrderWebSocketMiddleware
    {
        private readonly RequestDelegate _next;

        public OrderWebSocketMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, OrderService orderService)
        {
            if (!context.WebSockets.IsWebSocketRequest)
            {
                await _next(context);
                return;
            }

            using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
            var buffer = new byte[1024 * 4];
            var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            var message = Encoding.UTF8.GetString(buffer, 0, result.Count);

            string response = "";

            if (message.StartsWith("getOrdersByUser"))
            {
                var rest = message.Substring("getOrdersByUser:".Length);
                if (int.TryParse(rest, out var userId))
                {
                    var orders = await orderService.GetOrdersByUserAsync(userId);
                    response = JsonSerializer.Serialize(orders);
                }
                else
                {
                    response = JsonSerializer.Serialize(new { error = "Invalid or missing userId." });
                }
            }
            else if (message.StartsWith("placeOrder"))
            {
                // Verwerk het plaatsen van een bestelling
                try
                {
                    // Haal alles ná de eerste "placeOrder:" er wél volledig af
                    var jsonPayload = message.Substring("placeOrder:".Length);
                    var orderDto = JsonSerializer.Deserialize<OrderDTO.Create>(jsonPayload);

                    if (orderDto != null)
                    {
                        var createdOrder = await orderService.CreateOrderAsync(orderDto);
                        response = JsonSerializer.Serialize(createdOrder);
                    }
                    else
                    {
                        response = JsonSerializer.Serialize(new { error = "Invalid order data." });
                    }
                }
                catch (Exception ex)
                {
                    response = JsonSerializer.Serialize(new { error = $"Error placing order: {ex.Message}" });
                }
            }
            else
            {
                response = JsonSerializer.Serialize(new { error = "Unknown command." });
            }

            if (!string.IsNullOrEmpty(response))
            {
                await webSocket.SendAsync(
                    new ArraySegment<byte>(Encoding.UTF8.GetBytes(response)),
                    WebSocketMessageType.Text,
                    true,
                    CancellationToken.None);
            }

            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Done", CancellationToken.None);
        }
    }
}
