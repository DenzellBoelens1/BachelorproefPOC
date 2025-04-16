using Microsoft.AspNetCore.Components;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Webshop.Shared.DTOs;

namespace Webshop.Client.Services.Websockets
{
    public class OrderWebSocketService
    {
        private readonly NavigationManager _nav;

        public OrderWebSocketService(NavigationManager nav)
        {
            _nav = nav;
        }

        private Uri GetWebSocketUri() => new Uri("ws://localhost:5139/ws/order");

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

        public async Task<List<OrderDTO>> GetOrdersByUser(int userId)
        {
            var message = $"getOrdersByUser:{userId}";
            var json = await SendWebSocketMessageAsync(message);

            try
            {
                var orders = JsonSerializer.Deserialize<List<OrderDTO>>(json);
                return orders ?? new List<OrderDTO>();
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"JSON parse error: {ex.Message}");
                return new List<OrderDTO>();
            }
        }
    }
    
}
