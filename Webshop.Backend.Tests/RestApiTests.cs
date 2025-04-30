using System;
using System.Net;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.SignalR.Client;
using Xunit;
using Webshop.Backend;                     // Program class
using Webshop.Shared.DTOs;                 // DTOs
using HotChocolate.Execution;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using System.Linq;



namespace Webshop.Backend.Tests
{
    public class IntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public IntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory.WithWebHostBuilder(b => b.UseEnvironment("Test"));
            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task Rest_GetProducts_ReturnsOkAndList()
        {
            var res = await _client.GetAsync("/api/products");
            Assert.Equal(HttpStatusCode.OK, res.StatusCode);

            var products = await res.Content.ReadFromJsonAsync<ProductDTO.Index[]>();
            Assert.NotEmpty(products);
        }

        [Fact]
        public async Task Rest_UpdateStock_ReturnsUpdated()
        {
            var update = new ProductDTO.UpdateStock { ProductID = 1, InStock = 42 };
            var res = await _client.PutAsJsonAsync($"/api/products/{update.ProductID}/stock", update);
            Assert.Equal(HttpStatusCode.OK, res.StatusCode);

            var updated = await res.Content.ReadFromJsonAsync<ProductDTO.Index>();
            Assert.Equal(42, updated.InStock);
        }

        [Fact]
        public async Task Rest_GetOrdersByUser_ReturnsOk()
        {
            var res = await _client.GetAsync("/api/orders/user/1");
            Assert.Equal(HttpStatusCode.OK, res.StatusCode);

            var orders = await res.Content.ReadFromJsonAsync<OrderDTO.Index[]>();
            Assert.NotNull(orders);
        }
        [Fact]
        public async Task GraphQL_QueryProducts_NoErrors()
        {
            // Arrange
            var payload = new
            {
                query = "{ products { nodes { productID name inStock } } }"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/graphql", payload);

            // Assert HTTP 200
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Lees als JsonDocument
            using var json = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonDocument>();
            var root = json.RootElement;

            // Er mag geen "errors" property zijn
            Assert.False(root.TryGetProperty("errors", out _));

            // Er moet wel een "data" property zijn
            Assert.True(root.TryGetProperty("data", out var data));
            // En daarbinnen moeten we nodes kunnen vinden
            var products = data
                .GetProperty("products")
                .GetProperty("nodes")
                .EnumerateArray()
                .ToArray();

            Assert.NotEmpty(products);
        }

        [Fact]
        public async Task SignalR_ProductHub_CanReceiveProducts()
        {
            var url = _factory.Server.BaseAddress
                        .ToString().TrimEnd('/') + "/signalr/product";
            var connection = new HubConnectionBuilder()
                .WithUrl(url, opts => { opts.HttpMessageHandlerFactory = _ => _factory.Server.CreateHandler(); })
                .Build();

            string[] received = null;
            connection.On<ProductDTO.Index[]>("ReceiveProducts", data => received = data.Select(p => p.Name).ToArray());

            await connection.StartAsync();
            await connection.InvokeAsync("GetProducts", 1, 5, (string?)null);
            await Task.Delay(500);

            Assert.NotNull(received);
            Assert.NotEmpty(received);

            await connection.DisposeAsync();
        }

        [Fact]
        public async Task WebSocket_ProductMiddleware_ReceiveProducts()
        {
            var ws = new ClientWebSocket();
            var uri = new Uri(_factory.Server.BaseAddress.ToString().Replace("http", "ws") + "ws/product");
            await ws.ConnectAsync(uri, CancellationToken.None);

            // vraag eerste pagina
            var msg = Encoding.UTF8.GetBytes("getProducts:1:5:");
            await ws.SendAsync(msg, WebSocketMessageType.Text, true, CancellationToken.None);

            var buf = new byte[4096];
            var res = await ws.ReceiveAsync(buf, CancellationToken.None);
            var json = Encoding.UTF8.GetString(buf, 0, res.Count);

            Assert.Contains("ProductID", json);
            await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "done", CancellationToken.None);
        }
    }
}
