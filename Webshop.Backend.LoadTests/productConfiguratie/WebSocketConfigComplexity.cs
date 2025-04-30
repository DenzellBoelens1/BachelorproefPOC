using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NBomber.CSharp;
using NBomber.Contracts;

namespace Webshop.Backend.LoadTests
{
    public static class WebSocketConfigComplexity
    {
        /// <summary>
        /// dummy1 en dummy2 zorgen ervoor dat de signature matcht met je andere CreateScenario
        /// </summary>
        public static ScenarioProps CreateScenario(int dummy1, bool dummy2)
        {
            var scenarioName = $"ws_details_simple_opts{dummy1}_incl{dummy2}";
            return Scenario.Create(scenarioName, async context =>
            {
                int productId = new Random().Next(1, 101);

                using var ws = new ClientWebSocket();
                await ws.ConnectAsync(
                    new Uri("ws://localhost:5139/ws/product"),
                    CancellationToken.None
                );

                // Verzoek als plain text message
                var msg = $"getProductDetailsById:{productId}";
                var sendBytes = Encoding.UTF8.GetBytes(msg);
                await ws.SendAsync(
                    new ArraySegment<byte>(sendBytes),
                    WebSocketMessageType.Text,
                    endOfMessage: true,
                    CancellationToken.None
                );

                // Ontvang antwoord
                var buffer = new byte[8 * 1024];
                var result = await ws.ReceiveAsync(
                    new ArraySegment<byte>(buffer),
                    CancellationToken.None
                );

                // payload-grootte
                var receivedBytes = result.Count;

                await ws.CloseAsync(
                    WebSocketCloseStatus.NormalClosure,
                    "Done",
                    CancellationToken.None
                );

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
