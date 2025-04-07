using System.Net.WebSockets;
using System.Text.Json;
using System.Text;
using Webshop.Backend.Data;
using Webshop.Shared.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Webshop.Backend.Middleware
{
    public class ProductWebSocketMiddleware
    {
        private readonly RequestDelegate _next;

        public ProductWebSocketMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, AppDbContext dbContext)
        {
            if (context.Request.Path.StartsWithSegments("/productHub"))
            {
                await _next(context);
                return;
            }

            if (context.WebSockets.IsWebSocketRequest)
            {
                using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                var buffer = new byte[1024 * 4];
                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                var message = Encoding.UTF8.GetString(buffer, 0, result.Count);

                // 👇 Parsing: "getProducts:2:10"
                if (message.StartsWith("getProducts"))
                {
                    var parts = message.Split(':');
                    int page = parts.Length > 1 && int.TryParse(parts[1], out var p) ? p : 1;
                    int pageSize = parts.Length > 2 && int.TryParse(parts[2], out var ps) ? ps : 10;

                    int skip = (page - 1) * pageSize;

                    var products = await dbContext.Products
                        .Skip(skip)
                        .Take(pageSize)
                        .Select(p => new ProductDTO.Index
                        {
                            ProductID = p.ProductID,
                            Name = p.Name,
                            InStock = p.InStock
                        }).ToListAsync();

                    var response = JsonSerializer.Serialize(products);
                    var responseBytes = Encoding.UTF8.GetBytes(response);
                    await webSocket.SendAsync(new ArraySegment<byte>(responseBytes), WebSocketMessageType.Text, true, CancellationToken.None);
                }

                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Done", CancellationToken.None);
            }
            else
            {
                await _next(context);
            }
        }

        // 👇 Voeg een klein model toe binnenin dezelfde file of apart bestand
        public class WebSocketRequest
        {
            public string Action { get; set; } = "";
            public int Page { get; set; } = 1;
            public int PageSize { get; set; } = 10;
        }
    }
}