using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using NBomber.CSharp;
using NBomber.Contracts;
using Microsoft.AspNetCore.SignalR.Client;
using Webshop.Shared.DTOs;

namespace Webshop.Backend.LoadTests
{
    public static class SignalRConfigComplexity
    {
        /// <summary>
        /// dummy1 en dummy2 matchen signature van de andere CreateScenario
        /// </summary>
        public static ScenarioProps CreateScenario(int dummy1, bool dummy2)
        {
            var scenarioName = $"signalr_details_simple_opts{dummy1}_incl{dummy2}";
            return Scenario.Create(scenarioName, async context =>
            {
                int productId = new Random().Next(1, 101);

                // Zet de verbinding op
                var connection = new HubConnectionBuilder()
                    .WithUrl("http://localhost:5139/signalr/product")
                    .Build();

                await connection.StartAsync();

                // Wacht tot we het DTO-object binnenkrijgen
                var tcs = new TaskCompletionSource<ProductDTO.Details>();
                connection.On<ProductDTO.Details>(
                    "ReceiveProductDetails",
                    details => tcs.TrySetResult(details)
                );

                // Roep de hub‐methode aan
                await connection.InvokeAsync("GetProductDetails", productId);

                // Ontvangst blokkeren tot event fired
                var detailsDto = await tcs.Task;

                // Seriële JSON en grootte bepalen
                var json = JsonSerializer.Serialize(detailsDto);
                var receivedBytes = Encoding.UTF8.GetBytes(json).Length;

                await connection.StopAsync();

                return Response.Ok(sizeBytes: receivedBytes);
            })
            .WithLoadSimulations(
                Simulation.Inject(
                    rate: 50,
                    interval: TimeSpan.FromSeconds(1),
                    during: TimeSpan.FromMinutes(1)
                )
            );
        }
    }
}
