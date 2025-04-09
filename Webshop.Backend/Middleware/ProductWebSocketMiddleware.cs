using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Webshop.Backend.Services;
using Webshop.Shared.DTOs;

public class ProductWebSocketMiddleware
{
    private readonly RequestDelegate _next;

    public ProductWebSocketMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ProductService productService)
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

            string response = "";

            if (message.StartsWith("getProducts"))
            {
                var parts = message.Split(':');
                int page = parts.Length > 1 && int.TryParse(parts[1], out var p) ? p : 1;
                int pageSize = parts.Length > 2 && int.TryParse(parts[2], out var ps) ? ps : 10;
                string? search = parts.Length > 3 ? parts[3] : null;

                var products = await productService.GetProductsAsync(page, pageSize, search);
                response = JsonSerializer.Serialize(products);
            }
            else if (message.StartsWith("getProductById"))
            {
                var parts = message.Split(':');
                if (parts.Length > 1 && int.TryParse(parts[1], out var id))
                {
                    var product = await productService.GetProductIndexAsync(id);
                    response = product == null
                        ? JsonSerializer.Serialize(new { error = $"Product with ID {id} not found." })
                        : JsonSerializer.Serialize(product);
                }
            }
            else if (message.StartsWith("updateStock"))
            {
                var parts = message.Split(':');
                if (parts.Length == 3 &&
                    int.TryParse(parts[1], out var id) &&
                    int.TryParse(parts[2], out var inStock))
                {
                    var updated = await productService.UpdateStockAsync(id, inStock);
                    response = updated == null
                        ? JsonSerializer.Serialize(new { error = $"Product with ID {id} not found." })
                        : JsonSerializer.Serialize(updated);
                }
            }
            else if (message.StartsWith("getProductDetailsById"))
            {
                var parts = message.Split(':');
                if (parts.Length > 1 && int.TryParse(parts[1], out var id))
                {
                    var product = await productService.GetProductDetailsAsync(id);
                    response = product == null
                        ? JsonSerializer.Serialize(new { error = $"Product with ID {id} not found." })
                        : JsonSerializer.Serialize(product);
                }
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
        else
        {
            await _next(context);
        }
    }
}
