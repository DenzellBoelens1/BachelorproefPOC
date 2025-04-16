using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Webshop.Backend.Services;

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
                var parts = message.Split(':');
                if (parts.Length > 1 && int.TryParse(parts[1], out var userId))
                {
                    var orders = await orderService.GetOrdersByUserAsync(userId);
                    response = JsonSerializer.Serialize(orders);
                }
                else
                {
                    response = JsonSerializer.Serialize(new { error = "Invalid or missing userId." });
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
