using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Microsoft.AspNetCore.SignalR.Client;
using Webshop.Shared.DTOs;

namespace Webshop.Backend.Benchmarks
{
    [MemoryDiagnoser]
    public class PriceBenchmark
    {
        private readonly Uri _baseUrl = new Uri("http://localhost:5139"); // pas aan naar jouw API-url
        private HttpClient _httpClient = default!;
        private HubConnection _hubConnection = default!;
        private ClientWebSocket _webSocket = default!;

        [Params(50, 100, 200)]
        public int ConcurrentRequests;

        [GlobalSetup]
        public async Task Setup()
        {
            // REST/GraphQL client
            _httpClient = new HttpClient { BaseAddress = _baseUrl };

            // SignalR client
            _hubConnection = new HubConnectionBuilder()
                .WithUrl(new Uri(_baseUrl, "/signalr/product"))
                .Build();
            await _hubConnection.StartAsync();

            // WebSocket client
            _webSocket = new ClientWebSocket();
            await _webSocket.ConnectAsync(new Uri(_baseUrl, "/ws/product"), CancellationToken.None);
        }

        [Benchmark(Description = "REST CalculatePrice")]
        public async Task Rest_CalculatePrice()
        {
            var tasks = new Task[ConcurrentRequests];
            var payload = new { items = new[] { new { productId = 1, quantity = 3 } } };

            for (int i = 0; i < ConcurrentRequests; i++)
            {
                tasks[i] = _httpClient.PostAsJsonAsync("/api/cart/price", payload);
            }

            await Task.WhenAll(tasks);
        }

        [Benchmark(Description = "GraphQL CalculatePrice")]
        public async Task GraphQL_CalculatePrice()
        {
            var tasks = new Task[ConcurrentRequests];
            var gqlRequest = new
            {
                query = @"mutation CalculatePrice($productId: Int!, $quantity: Int!, $selectedOptionIds: [Int!]!, $optionValues: JSON!, $customText: String) {
                    calculatePrice(productId: $productId, quantity: $quantity, selectedOptionIds: $selectedOptionIds, optionValues: $optionValues, customText: $customText) {
                      unitPrice
                      totalPrice
                    }
                  }",
                variables = new
                {
                    productId = 1,
                    quantity = 3,
                    selectedOptionIds = Array.Empty<int>(),
                    optionValues = new Dictionary<int, string>(),
                    customText = (string?)null
                }
            };

            for (int i = 0; i < ConcurrentRequests; i++)
            {
                tasks[i] = _httpClient.PostAsJsonAsync("/graphql", gqlRequest);
            }

            await Task.WhenAll(tasks);
        }

        [Benchmark(Description = "SignalR CalculatePrice")]
        public async Task SignalR_CalculatePrice()
        {
            var tasks = new Task[ConcurrentRequests];

            for (int i = 0; i < ConcurrentRequests; i++)
            {
                tasks[i] = _hubConnection.InvokeAsync<PriceDTO>(
                    "CalculatePrice",
                    1,
                    3,
                    Array.Empty<int>(),
                    new Dictionary<int, string>(),
                    null
                );
            }

            await Task.WhenAll(tasks);
        }

        [Benchmark(Description = "WebSocket CalculatePrice")]
        public async Task WebSocket_CalculatePrice()
        {
            var tasks = new Task[ConcurrentRequests];

            for (int i = 0; i < ConcurrentRequests; i++)
            {
                tasks[i] = SendWebSocketRequest();
            }

            await Task.WhenAll(tasks);
        }

        private async Task SendWebSocketRequest()
        {
            var payload = new
            {
                productId = 1,
                quantity = 3,
                selectedOptionIds = Array.Empty<int>(),
                optionValues = new Dictionary<int, string>(),
                customText = (string?)null
            };
            var json = JsonSerializer.Serialize(payload);
            var message = $"calculatePrice:{json}";
            var bytes = Encoding.UTF8.GetBytes(message);

            await _webSocket.SendAsync(
                new ArraySegment<byte>(bytes),
                WebSocketMessageType.Text,
                endOfMessage: true,
                cancellationToken: CancellationToken.None
            );

            var buffer = new byte[4 * 1024];
            var result = await _webSocket.ReceiveAsync(
                new ArraySegment<byte>(buffer),
                CancellationToken.None
            );
        }

        [GlobalCleanup]
        public async Task Cleanup()
        {
            await _hubConnection.DisposeAsync();
            _webSocket.Dispose();
            _httpClient.Dispose();
        }

        public static void Main(string[] args) =>
            BenchmarkRunner.Run<PriceBenchmark>();
    }
}
