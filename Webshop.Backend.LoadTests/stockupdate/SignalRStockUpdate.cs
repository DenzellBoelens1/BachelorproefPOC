// SignalRStockUpdate.cs
using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using NBomber.CSharp;
using NBomber.Contracts;
using Webshop.Shared.DTOs;

namespace Webshop.Backend.LoadTests
{
    public static class SignalRStockUpdate
    {
        public static ScenarioProps CreateScenario()
        {
            var rnd = new Random();
            return Scenario.Create("signalr_stock_update", async context =>
            {
                int id = rnd.Next(1, 101);
                int stock = rnd.Next(0, 501);

                var connection = new HubConnectionBuilder()
                    .WithUrl("http://localhost:5139/signalr/product")
                    .Build();

                await connection.StartAsync();

                // TaskCompletionSource returns the JSON payload for size calculation
                var tcs = new TaskCompletionSource<string>();
                connection.On<ProductDTO.Index>(
                    "ReceiveStockUpdated",
                    dto =>
                    {
                        if (dto.ProductID == id && dto.InStock == stock)
                        {
                            string json = JsonSerializer.Serialize(dto);
                            tcs.TrySetResult(json);
                        }
                    }
                );

                await connection.InvokeAsync("UpdateStock", id, stock);

                // wacht op ontvangst of timeout
                var completed = await Task.WhenAny(tcs.Task, Task.Delay(5000));
                if (completed == tcs.Task)
                {
                    string json = await tcs.Task;
                    var bytes = Encoding.UTF8.GetBytes(json);
                    await connection.StopAsync();
                    return Response.Ok(sizeBytes: bytes.Length);
                }
                else
                {
                    await connection.StopAsync();
                    return Response.Fail();
                }
            })
            .WithLoadSimulations(
                Simulation.Inject(
                    rate: 100,
                    during: TimeSpan.FromMinutes(1),
                    interval: TimeSpan.FromSeconds(1)
                )
            );
        }
    }
}
