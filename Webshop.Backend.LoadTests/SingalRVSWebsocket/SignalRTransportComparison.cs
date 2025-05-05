// File: SignalRTransportComparison.cs
using System;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.Http.Connections;
using NBomber.CSharp;
using NBomber.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace Webshop.Backend.LoadTests
{
    public static class SignalRTransportComparison
    {
        private static readonly string BaseUrl = "http://localhost:5139";

        public static ScenarioProps CreateWebSocketScenario()
        {
            // 1) Bouw de HubConnection met alleen WebSockets (JSON-protocol)
            var wsConnection = new HubConnectionBuilder()
                .WithUrl($"{BaseUrl}/signalr/product", options =>
                {
                    options.Transports = HttpTransportType.WebSockets;
                })
                // expliciet JSON-protocol (default is al JSON, maar voor de duidelijkheid)
                .AddJsonProtocol()
                .Build();

            // 2) Stel de timeouts in op de connection zelf
            wsConnection.ServerTimeout = TimeSpan.FromSeconds(30);
            wsConnection.HandshakeTimeout = TimeSpan.FromSeconds(15);

            // 3) Start de verbinding vóór de load test
            wsConnection.StartAsync().GetAwaiter().GetResult();

            // 4) Definieer het NBomber-scenario
            return Scenario.Create("signalr_ws_transport", async context =>
            {
                // voer CalculatePrice uit
                var dto = await wsConnection.InvokeAsync<Webshop.Shared.DTOs.PriceDTO>(
                    "CalculatePrice",
                    1, 3,
                    Array.Empty<int>(),
                    new System.Collections.Generic.Dictionary<int, string>(),
                    null
                );

                // meet payload-grootte
                var json = JsonSerializer.Serialize(dto);
                var size = Encoding.UTF8.GetBytes(json).Length;
                return Response.Ok(sizeBytes: size);
            })
            .WithWarmUpDuration(TimeSpan.FromSeconds(15))  // warm-up
           .WithLoadSimulations(
    Simulation.Inject(rate: 50, during: TimeSpan.FromMinutes(1), interval: TimeSpan.FromSeconds(1)),
    Simulation.Inject(rate: 200, during: TimeSpan.FromMinutes(1), interval: TimeSpan.FromSeconds(5))
)

            .WithClean(async cleanCtx =>
            {
                await wsConnection.StopAsync();
                await wsConnection.DisposeAsync();
            });
        }

        public static ScenarioProps CreateSseScenario()
        {
            // 1) Bouw de HubConnection met SSE (JSON-protocol)
            var sseConnection = new HubConnectionBuilder()
                .WithUrl($"{BaseUrl}/signalr/product", options =>
                {
                    options.Transports = HttpTransportType.ServerSentEvents;
                })
                .AddJsonProtocol()
                .Build();

            sseConnection.ServerTimeout = TimeSpan.FromSeconds(30);
            sseConnection.HandshakeTimeout = TimeSpan.FromSeconds(15);

            sseConnection.StartAsync().GetAwaiter().GetResult();

            return Scenario.Create("signalr_sse_transport", async context =>
            {
                var dto = await sseConnection.InvokeAsync<Webshop.Shared.DTOs.PriceDTO>(
                    "CalculatePrice",
                    1, 3,
                    Array.Empty<int>(),
                    new System.Collections.Generic.Dictionary<int, string>(),
                    null
                );

                var json = JsonSerializer.Serialize(dto);
                var size = Encoding.UTF8.GetBytes(json).Length;
                return Response.Ok(sizeBytes: size);
            })
            .WithWarmUpDuration(TimeSpan.FromSeconds(15))
            .WithLoadSimulations(
    Simulation.Inject(rate: 50, during: TimeSpan.FromMinutes(1), interval: TimeSpan.FromSeconds(1)),
    Simulation.Inject(rate: 200, during: TimeSpan.FromMinutes(1), interval: TimeSpan.FromSeconds(5))
)

            .WithClean(async cleanCtx =>
            {
                await sseConnection.StopAsync();
                await sseConnection.DisposeAsync();
            });
        }
    }
}
