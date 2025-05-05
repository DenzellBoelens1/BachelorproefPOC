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

                var tcs = new TaskCompletionSource<(int sendSize, int recvSize)>();
                connection.On<ProductDTO.Index>(
                    "ReceiveStockUpdated",
                    dto =>
                    {
                        if (dto.ProductID == id && dto.InStock == stock)
                        {
                            var recvJson = JsonSerializer.Serialize(dto);
                            var recvSize = Encoding.UTF8.GetByteCount(recvJson);
                            tcs.TrySetResult((0, recvSize));
                        }
                    }
                );

                var sendJson = JsonSerializer.Serialize(new { id, stock });
                var sendBytes = Encoding.UTF8.GetByteCount(sendJson);
                await connection.InvokeAsync("UpdateStock", id, stock);

                var completed = await Task.WhenAny(tcs.Task, Task.Delay(5000));
                if (completed == tcs.Task)
                {
                    var (_, recvSize) = await tcs.Task;
                    await connection.StopAsync();
                    return Response.Ok(sizeBytes: sendBytes + recvSize);
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
