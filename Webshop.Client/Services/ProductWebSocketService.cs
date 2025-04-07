using Microsoft.AspNetCore.Components;
using System.Net.WebSockets;
using System.Text.Json;
using System.Text;
using Webshop.Shared.DTOs;

namespace Webshop.Client.Services
{
    public class ProductWebSocketService
    {
        private readonly NavigationManager _nav;

        public ProductWebSocketService(NavigationManager nav) => _nav = nav;

        public async Task<List<ProductDTO.Index>> GetProducts(int page = 1, int pageSize = 10)
        {
            var wsUri = new Uri("ws://localhost:5139/ws");

            using var client = new ClientWebSocket();
            await client.ConnectAsync(wsUri, CancellationToken.None);

            // 👇 Simpele string met paginatie
            var message = $"getProducts:{page}:{pageSize}";
            var sendBuffer = Encoding.UTF8.GetBytes(message);

            await client.SendAsync(new ArraySegment<byte>(sendBuffer), WebSocketMessageType.Text, true, CancellationToken.None);

            // ✅ Volledig bericht opvangen
            var buffer = new byte[4096];
            var fullMessage = new StringBuilder();

            WebSocketReceiveResult result;
            do
            {
                result = await client.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                fullMessage.Append(Encoding.UTF8.GetString(buffer, 0, result.Count));
            }
            while (!result.EndOfMessage);

            var json = fullMessage.ToString();
            return JsonSerializer.Deserialize<List<ProductDTO.Index>>(json) ?? new();
        }

    }
}
