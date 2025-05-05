// File: SecurityTests.cs
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.WebSockets;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Webshop.Backend.Controllers;
using Webshop.Backend.Data;
using Webshop.Backend.Hubs;
using Webshop.Backend.Middleware;
using Webshop.Backend.Services;
using Xunit;
using Microsoft.AspNetCore.WebSockets;

namespace Webshop.Backend.Tests.Security
{
    public class SecurityTests : IAsyncLifetime
    {
        private TestServer _server;
        private HttpClient _client;
        private readonly Uri _wsUri = new("ws://localhost/ws/product");
        private readonly Uri _signalRUri = new("http://localhost/hub/product");

        public async Task InitializeAsync()
        {
            var builder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase("SecDb"));
                    services.AddScoped<ProductService>();

                    services.AddControllers()
                        .AddApplicationPart(typeof(ProductsController).Assembly)
                        .AddControllersAsServices();

                    services.AddAuthentication("TestScheme")
                        .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                            "TestScheme", opts => { });
                    services.AddAuthorization();

                    services.AddWebSockets(options => { });
                    services.AddSignalR();
                })
                .Configure(app =>
                {
                    app.UseRouting();
                    app.UseAuthentication();
                    app.UseAuthorization();

                    app.UseWebSockets();
                    app.UseMiddleware<ProductWebSocketMiddleware>();

                    app.UseEndpoints(endpoints =>
                    {
                        endpoints.MapControllers()
                                 .RequireAuthorization();
                        endpoints.MapHub<ProductHub>("/hub/product")
                                 .RequireAuthorization();
                    });
                });

            _server = new TestServer(builder);
            _client = _server.CreateClient();
        }

        public Task DisposeAsync()
        {
            _client.Dispose();
            _server.Dispose();
            return Task.CompletedTask;
        }

        [Fact]
        public async Task Rest_WithoutAuth_ReturnsUnauthorized()
        {
            var response = await _client.GetAsync("/api/products");
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Rest_WithAuth_ReturnsSuccess()
        {
            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("TestScheme");
            var response = await _client.GetAsync("/api/products");
            Assert.InRange((int)response.StatusCode, 200, 299);
        }


        [Fact]
        public async Task WebSocket_WithAuth_SucceedsHandshake()
        {
            var wsClient = _server.CreateWebSocketClient();
            wsClient.ConfigureRequest = request =>
            {
                request.Headers["Authorization"] = "TestScheme";
            };

            using var ws = await wsClient.ConnectAsync(_wsUri, CancellationToken.None);
            Assert.Equal(WebSocketState.Open, ws.State);
            await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Done", CancellationToken.None);
        }

        [Fact]
        public async Task SignalR_WithoutAuth_ThrowsHttpRequestException()
        {
            var connection = new HubConnectionBuilder()
                .WithUrl(_signalRUri, options =>
                {
                    options.HttpMessageHandlerFactory = _ => _server.CreateHandler();
                })
                .Build();

            await Assert.ThrowsAsync<HttpRequestException>(
                () => connection.StartAsync());
        }

        [Fact]
        public async Task SignalR_WithAuth_CanConnect()
        {
            var connection = new HubConnectionBuilder()
                .WithUrl(_signalRUri, options =>
                {
                    options.HttpMessageHandlerFactory = _ => _server.CreateHandler();
                    options.Headers["Authorization"] = "TestScheme";
                })
                .Build();

            await connection.StartAsync();
            Assert.Equal(HubConnectionState.Connected, connection.State);
            await connection.StopAsync();
        }
    }

    public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public TestAuthHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.TryGetValue("Authorization", out var value) ||
                value != "TestScheme")
            {
                return Task.FromResult(AuthenticateResult.Fail("Invalid or missing header"));
            }

            var claims = new[] { new Claim(ClaimTypes.Name, "testuser") };
            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
