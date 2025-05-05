using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.WebSockets;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Microsoft.AspNetCore.SignalR.Client;
using Webshop.Shared.DTOs;

namespace Webshop.Backend.Benchmarks.Price
{
    [MemoryDiagnoser]
    public class PriceBenchmark
    {
        private readonly Uri _baseUrl = new Uri("http://localhost:5139"); // pas aan naar jouw API-url
        private HttpClient _httpClient = default!;
        private HubConnection _hubConnection = default!;

        [Params(50, 100, 200)]
        public int ConcurrentRequests { get; set; }

        [GlobalSetup]
        public async Task Setup()
        {
            // 1) HTTP‐client met connection‐pooling
            var handler = new SocketsHttpHandler
            {
                MaxConnectionsPerServer = ConcurrentRequests,
                PooledConnectionLifetime = TimeSpan.FromSeconds(30),
                PooledConnectionIdleTimeout = TimeSpan.FromSeconds(15)
            };
            _httpClient = new HttpClient(handler) { BaseAddress = _baseUrl };

            // 2) SignalR‐client die dezelfde handler hergebruikt
            var signalRUrl = new UriBuilder(_baseUrl) { Path = "/signalr/product" }.Uri;
            _hubConnection = new HubConnectionBuilder()
                .WithUrl(signalRUrl, options =>
                {
                    options.HttpMessageHandlerFactory = _ => handler;
                })
                .Build();
            await _hubConnection.StartAsync();
        }

        // HTTP‐helper die fouten negeert
        private async Task SafePostAsync(string url, object payload)
        {
            try
            {
                await _httpClient.PostAsJsonAsync(url, payload);
            }
            catch (HttpRequestException)
            {
                // benchmark mag gewoon door
            }
        }

        [Benchmark(Description = "REST CalculatePrice")]
        public async Task Rest_CalculatePrice()
        {
            var tasks = new Task[ConcurrentRequests];
            var payload = new { items = new[] { new { productId = 1, quantity = 3 } } };

            for (int i = 0; i < ConcurrentRequests; i++)
                tasks[i] = SafePostAsync("/api/cart/price", payload);

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
                tasks[i] = SafePostAsync("/graphql", gqlRequest);

            await Task.WhenAll(tasks);
        }

        // SignalR‐helper die fouten negeert
        private async Task SafeInvokePriceAsync()
        {
            try
            {
                await _hubConnection.InvokeAsync<PriceDTO>(
                    "CalculatePrice",
                    1,
                    3,
                    Array.Empty<int>(),
                    new Dictionary<int, string>(),
                    null
                );
            }
            catch
            {
                // benchmark mag door
            }
        }

        [Benchmark(Description = "SignalR CalculatePrice")]
        public async Task SignalR_CalculatePrice()
        {
            var tasks = new Task[ConcurrentRequests];
            for (int i = 0; i < ConcurrentRequests; i++)
                tasks[i] = SafeInvokePriceAsync();
            await Task.WhenAll(tasks);
        }

        [Benchmark(Description = "WebSocket CalculatePrice")]
        public async Task WebSocket_CalculatePrice()
        {
            var tasks = new Task[ConcurrentRequests];
            for (int i = 0; i < ConcurrentRequests; i++)
                tasks[i] = SendWebSocketRequest();
            await Task.WhenAll(tasks);
        }

        // Deze methode opent per call een verse socket en doet netjes een close‐handshake
        private async Task SendWebSocketRequest()
        {
            using var ws = new ClientWebSocket();
            var wsScheme = _baseUrl.Scheme == Uri.UriSchemeHttps ? "wss" : "ws";
            var wsUrl = new UriBuilder(_baseUrl) { Scheme = wsScheme, Path = "/ws/product" }.Uri;

            // Open verbinding
            await ws.ConnectAsync(wsUrl, CancellationToken.None);

            // Verstuur payload
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
            await ws.SendAsync(
                new ArraySegment<byte>(bytes),
                WebSocketMessageType.Text,
                endOfMessage: true,
                cancellationToken: CancellationToken.None
            );

            // Ontvang antwoord (alleen Text‐frames)
            var buffer = new byte[4 * 1024];
            var result = await ws.ReceiveAsync(
                new ArraySegment<byte>(buffer),
                CancellationToken.None
            );
            if (result.MessageType == WebSocketMessageType.Text)
            {
                // optioneel: var response = Encoding.UTF8.GetString(buffer, 0, result.Count);
            }

            // Doe een nette close‐handshake
            if (ws.State == WebSocketState.Open ||
                ws.State == WebSocketState.CloseReceived)
            {
                await ws.CloseAsync(
                    WebSocketCloseStatus.NormalClosure,
                    "Client closing",
                    CancellationToken.None
                );
            }
        }

        [GlobalCleanup]
        public async Task Cleanup()
        {
            await _hubConnection.DisposeAsync();
            _httpClient.Dispose();
        }


    }
}
