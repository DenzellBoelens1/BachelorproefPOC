// File: ProductWebSocketMiddlewareTests.cs
using System;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Webshop.Backend.Data;
using Webshop.Backend.Middleware;
using Webshop.Backend.Services;
using Webshop.Shared.Models;
using Xunit;

namespace Webshop.Backend.Tests.Middleware
{
    public class ProductWebSocketMiddlewareTests : IAsyncLifetime
    {
        private TestServer _server = default!;
        private HttpClient _httpClient = default!;
        private Uri _wsUri = new("ws://localhost/ws/product");

        public async Task InitializeAsync()
        {
            // Start een in-memory server met alleen onze middleware
            var builder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    // In‐memory EF
                    services.AddDbContext<AppDbContext>(o =>
                        o.UseInMemoryDatabase("WsTestDb"));
                    services.AddScoped<ProductService>();
                })
                .Configure(app =>
                {
                    app.UseWebSockets();
                    app.UseMiddleware<ProductWebSocketMiddleware>();
                });

            _server = new TestServer(builder);
            _httpClient = _server.CreateClient();
        }

        public Task DisposeAsync()
        {
            _httpClient?.Dispose();
            _server?.Dispose();
            return Task.CompletedTask;
        }

        [Fact]
        public async Task MalformedMessage_ReturnsErrorJson()
        {
            using var ws = await _server
                .CreateWebSocketClient()
                .ConnectAsync(_wsUri, CancellationToken.None);

            // Verstuur malformed payload
            var msg = Encoding.UTF8.GetBytes("getProductById:abc");
            await ws.SendAsync(
                new ArraySegment<byte>(msg),
                WebSocketMessageType.Text,
                endOfMessage: true,
                CancellationToken.None);

            // Ontvang de response
            var buf = new byte[1024];
            var res = await ws.ReceiveAsync(
                new ArraySegment<byte>(buf),
                CancellationToken.None);

            var text = Encoding.UTF8.GetString(buf, 0, res.Count);

            // oude assert: Assert.Contains("abc", text);
            Assert.Contains("\"error\"", text);
            Assert.Contains("Invalid product ID.", text);
        }

        [Fact]
        public async Task GetProductsCommand_ReturnsProductsJson()
        {
            // Seed de DB via scope
            using (var scope = _server.Services.CreateScope())
            {
                var ctx = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                ctx.Products.Add(new Product { ProductID = 1, Name = "A", InStock = 5, MinStock = 0, BasePrice = 1m, Description = "" });
                ctx.Products.Add(new Product { ProductID = 2, Name = "B", InStock = 6, MinStock = 0, BasePrice = 2m, Description = "" });
                await ctx.SaveChangesAsync();
            }

            using var ws = await _server
                .CreateWebSocketClient()
                .ConnectAsync(_wsUri, CancellationToken.None);

            // Verstuur getProducts:page=1,pageSize=10
            var cmd = "getProducts:1:10:";
            var msg = Encoding.UTF8.GetBytes(cmd);
            await ws.SendAsync(
                new ArraySegment<byte>(msg),
                WebSocketMessageType.Text,
                true,
                CancellationToken.None);

            // Ontvang
            var buf = new byte[4096];
            var res = await ws.ReceiveAsync(
                new ArraySegment<byte>(buf),
                CancellationToken.None);

            var json = Encoding.UTF8.GetString(buf, 0, res.Count);
            Assert.Contains("\"ProductID\":1", json);
            Assert.Contains("\"Name\":\"A\"", json);
            Assert.Contains("\"ProductID\":2", json);
            Assert.Contains("\"Name\":\"B\"", json);
        }
    }
}
