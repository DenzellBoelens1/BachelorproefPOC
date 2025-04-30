// ProductWebSocketMiddleware.cs
using System;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
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
        // Alleen intercept WebSocket-requests naar /ws/product
        if (!context.Request.Path.StartsWithSegments("/ws/product") || !context.WebSockets.IsWebSocketRequest)
        {
            await _next(context);
            return;
        }

        using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
        var buffer = new byte[1024 * 4];
        var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        var message = Encoding.UTF8.GetString(buffer, 0, result.Count);

        string response = string.Empty;

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
                var details = await productService.GetProductDetailsAsync(id);
                response = details == null
                    ? JsonSerializer.Serialize(new { error = $"Product with ID {id} not found." })
                    : JsonSerializer.Serialize(details);
            }
        }
        else if (message.StartsWith("calculatePrice:"))
        {
            // Payload volgt na 'calculatePrice:' als JSON
            var payloadJson = message.Substring("calculatePrice:".Length);
            using var doc = JsonDocument.Parse(payloadJson);
            var root = doc.RootElement;
            int productId = root.GetProperty("productId").GetInt32();
            int quantity = root.GetProperty("quantity").GetInt32();

            // selectedOptionIds
            var selectedOptionIds = new List<int>();
            foreach (var el in root.GetProperty("selectedOptionIds").EnumerateArray())
                selectedOptionIds.Add(el.GetInt32());

            // optionValues
            var optionValues = new Dictionary<int, string>();
            foreach (var prop in root.GetProperty("optionValues").EnumerateObject())
                optionValues[int.Parse(prop.Name)] = prop.Value.GetString() ?? string.Empty;

            string? customText = null;
            if (root.TryGetProperty("customText", out var ctProp) && ctProp.ValueKind == JsonValueKind.String)
                customText = ctProp.GetString();

            // Bereken prijs
            var resultTuple = await productService.CalculatePriceAsync(
                productId,
                quantity,
                selectedOptionIds,
                optionValues,
                customText);

            decimal unit = resultTuple.UnitPrice;
            decimal total = resultTuple.TotalPrice;

            var priceDto = new PriceDTO { UnitPrice = unit, TotalPrice = total };
            response = JsonSerializer.Serialize(priceDto);
        }

        // Verstuur response indien gevuld
        if (!string.IsNullOrEmpty(response))
        {
            var respBytes = Encoding.UTF8.GetBytes(response);
            await webSocket.SendAsync(
                new ArraySegment<byte>(respBytes),
                WebSocketMessageType.Text,
                true,
                CancellationToken.None);
        }

        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Done", CancellationToken.None);
    }
}
