using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;
using NBomber.Contracts;
using NBomber.CSharp;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Webshop.Shared.DTOs;

namespace Webshop.Backend.LoadTests.priceTest
{
    public static class PriceLoadTestScenarios
    {
        // ──────────────────────────────────────────────── REST ────────────────────────────────────────────────

        private static readonly HttpClient _restClient = new HttpClient
        {
            BaseAddress = new Uri("http://localhost:5139")
        };

        public static ScenarioProps RestPriceScenario() =>
            Scenario.Create("rest_calculate_price", async context =>
            {
                var rnd = new Random();
                int productId = rnd.Next(1, 101);
                int quantity = rnd.Next(1, 6);

                var req = new PriceCalculationRequestDTO
                {
                    Quantity = quantity,
                    SelectedOptionIds = new List<int>(),
                    OptionValues = new Dictionary<int, string>(),
                    CustomText = null
                };

                var res = await _restClient.PostAsJsonAsync(
                    $"/api/products/{productId}/pricing", req);

                if (!res.IsSuccessStatusCode)
                    return Response.Fail();

                var bytes = await res.Content.ReadAsByteArrayAsync();
                return Response.Ok(sizeBytes: bytes.Length);
            })
            .WithLoadSimulations(
                Simulation.Inject(rate: 50,
                                  during: TimeSpan.FromMinutes(1),
                                  interval: TimeSpan.FromSeconds(1))
            );

        // ──────────────────────────────────────────── GraphQL ────────────────────────────────────────────

        private static readonly HttpClient _gqlClient = new HttpClient
        {
            BaseAddress = new Uri("http://localhost:5139")
        };

        public static ScenarioProps GraphQLPriceScenario() =>
            Scenario.Create("graphql_calculate_price", async context =>
            {
                var rnd = new Random();
                int productId = rnd.Next(1, 101);
                int quantity = rnd.Next(1, 6);

                // Inline‐mutation met lege lijsten
                var mutation = $@"
                        mutation {{
                          calculatePrice(
                            productId: {productId},
                            quantity: {quantity},
                            selectedOptionIds: [],
                            optionValues: [],
                            customText: null
                          ) {{
                            unitPrice
                            totalPrice
                          }}
                        }}";

                var payload = new { query = mutation };
                var res = await _gqlClient.PostAsJsonAsync("/graphql", payload);

                if (!res.IsSuccessStatusCode)
                    return Response.Fail();

                var bytes = await res.Content.ReadAsByteArrayAsync();
                return Response.Ok(sizeBytes: bytes.Length);
            })
            .WithLoadSimulations(
                Simulation.Inject(rate: 50,
                                  during: TimeSpan.FromMinutes(1),
                                  interval: TimeSpan.FromSeconds(1))
            );

        // ─────────────────────────────────────────── SignalR ───────────────────────────────────────────

        public static ScenarioProps SignalRPriceScenario() =>
            Scenario.Create("signalr_calculate_price", async context =>
            {
                var rnd = new Random();
                int productId = rnd.Next(1, 101);
                int quantity = rnd.Next(1, 6);

                var connection = new HubConnectionBuilder()
                    .WithUrl("http://localhost:5139/signalr/product",
                             opts => opts.Transports = HttpTransportType.WebSockets)
                    .WithAutomaticReconnect()
                    .Build();
                await connection.StartAsync();

                var dto = await connection.InvokeAsync<PriceDTO>(
                    "CalculatePrice",
                    productId, quantity,
                    new List<int>(),
                    new Dictionary<int, string>(),
                    null
                );

                var json = JsonSerializer.Serialize(dto);
                var bytes = Encoding.UTF8.GetBytes(json).Length;

                await connection.StopAsync();
                return Response.Ok(sizeBytes: bytes);
            })
            .WithLoadSimulations(
                Simulation.Inject(rate: 50,
                                  during: TimeSpan.FromMinutes(1),
                                  interval: TimeSpan.FromSeconds(1))
            );

        // ───────────────────────────────────────── WebSockets ─────────────────────────────────────────

        public static ScenarioProps WebSocketPriceScenario() =>
            Scenario.Create("ws_calculate_price", async context =>
            {
                var rnd = new Random();
                int productId = rnd.Next(1, 101);
                int quantity = rnd.Next(1, 6);

                using var ws = new ClientWebSocket();
                await ws.ConnectAsync(new Uri("ws://localhost:5139/ws/product"), CancellationToken.None);

                var payload = JsonSerializer.Serialize(new
                {
                    productId,
                    quantity,
                    selectedOptionIds = new List<int>(),
                    optionValues = new Dictionary<int, string>(),
                    customText = (string?)null
                });
                var message = $"calculatePrice:{payload}";
                var msgBytes = Encoding.UTF8.GetBytes(message);

                await ws.SendAsync(msgBytes,
                                   WebSocketMessageType.Text,
                                   true,
                                   CancellationToken.None);

                var buf = new byte[4096];
                var result = await ws.ReceiveAsync(buf, CancellationToken.None);
                await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "done", CancellationToken.None);

                return Response.Ok(sizeBytes: result.Count);
            })
            .WithLoadSimulations(
                Simulation.Inject(rate: 50,
                                  during: TimeSpan.FromMinutes(1),
                                  interval: TimeSpan.FromSeconds(1))
            );
    }
}
