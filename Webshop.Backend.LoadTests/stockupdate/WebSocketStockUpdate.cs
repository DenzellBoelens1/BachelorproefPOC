using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NBomber.CSharp;
using NBomber.Contracts;

namespace Webshop.Backend.LoadTests
{
    public static class WebSocketStockUpdate
    {
        public static ScenarioProps CreateScenario()
        {
            var rnd = new Random();
            return Scenario.Create("ws_stock_update", async context =>
            {
                int id = rnd.Next(1, 101);
                int stock = rnd.Next(0, 501);

                using var ws = new ClientWebSocket();
                await ws.ConnectAsync(new Uri("ws://localhost:5139/ws/product"), CancellationToken.None);

                var msg = $"updateStock:{id}:{stock}";
                var sendBytes = Encoding.UTF8.GetByteCount(msg);
                await ws.SendAsync(
                    new ArraySegment<byte>(Encoding.UTF8.GetBytes(msg)),
                    WebSocketMessageType.Text,
                    true,
                    CancellationToken.None
                );

                var buffer = new byte[4096];
                var result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                var recvBytes = result.Count;

                await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "done", CancellationToken.None);

                return Response.Ok(sizeBytes: sendBytes + recvBytes);
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
