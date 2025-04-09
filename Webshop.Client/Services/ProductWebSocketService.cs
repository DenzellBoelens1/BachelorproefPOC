using Microsoft.AspNetCore.Components;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Webshop.Shared.DTOs;

namespace Webshop.Client.Services
{
    public class ProductWebSocketService
    {
        private readonly NavigationManager _nav;

        public ProductWebSocketService(NavigationManager nav)
        {
            _nav = nav;
        }

        private Uri GetWebSocketUri() => new Uri("ws://localhost:5139/ws");



        private async Task<string> SendWebSocketMessageAsync(string message)
        {
            var uri = GetWebSocketUri();

            using var client = new ClientWebSocket();
            await client.ConnectAsync(uri, CancellationToken.None);

            var buffer = Encoding.UTF8.GetBytes(message);
            await client.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);

            var receiveBuffer = new byte[4096];
            var fullMessage = new StringBuilder();
            WebSocketReceiveResult result;

            do
            {
                result = await client.ReceiveAsync(new ArraySegment<byte>(receiveBuffer), CancellationToken.None);
                fullMessage.Append(Encoding.UTF8.GetString(receiveBuffer, 0, result.Count));
            }
            while (!result.EndOfMessage);

            return fullMessage.ToString();
        }

        public async Task<List<ProductDTO.Index>> GetProducts(int page = 1, int pageSize = 10, string? search = null)
        {
            // Gebruik lege string als er geen zoekterm is
            var safeSearch = string.IsNullOrWhiteSpace(search) ? "" : search;
            var message = $"getProducts:{page}:{pageSize}:{safeSearch}";
            var json = await SendWebSocketMessageAsync(message);

            return JsonSerializer.Deserialize<List<ProductDTO.Index>>(json) ?? new();
        }

        public async Task<ProductDTO.Index?> GetProductById(int id)
        {
            var message = $"getProductById:{id}";
            var json = await SendWebSocketMessageAsync(message);

            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("error", out var errorProp))
            {
                Console.WriteLine($"WebSocket error: {errorProp.GetString()}");
                return null;
            }

            return JsonSerializer.Deserialize<ProductDTO.Index>(json);
        }

        public async Task<ProductDTO.Details?> GetProductDetailsById(int id)
        {
            var message = $"getProductDetailsById:{id}";
            var json = await SendWebSocketMessageAsync(message);

            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("error", out var errorProp))
            {
                Console.WriteLine($"WebSocket error: {errorProp.GetString()}");
                return null;
            }

            return JsonSerializer.Deserialize<ProductDTO.Details>(json);
        }

        public async Task<ProductDTO.Index?> UpdateStock(ProductDTO.UpdateStock update)
        {
            var message = $"updateStock:{update.ProductID}:{update.InStock}";
            var json = await SendWebSocketMessageAsync(message);

            return JsonSerializer.Deserialize<ProductDTO.Index>(json);
        }
    }
}
