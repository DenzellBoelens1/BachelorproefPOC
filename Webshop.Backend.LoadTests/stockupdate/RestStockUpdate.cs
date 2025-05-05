using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using NBomber.CSharp;
using NBomber.Contracts;

namespace Webshop.Backend.LoadTests
{
    public static class RestStockUpdate
    {
        private static readonly HttpClient client = new HttpClient { BaseAddress = new Uri("http://localhost:5139") };

        public static ScenarioProps CreateScenario()
        {
            var rnd = new Random();
            return Scenario.Create("rest_stock_update", async context =>
            {
                int id = rnd.Next(1, 101);
                int stock = rnd.Next(0, 501);

                var payload = new { ProductID = id, InStock = stock };
                var reqJson = JsonSerializer.Serialize(payload);
                var reqBytes = Encoding.UTF8.GetByteCount(reqJson);

                try
                {
                    var res = await client.PutAsJsonAsync($"/api/products/{id}/stock", payload);
                    if (!res.IsSuccessStatusCode)
                        return Response.Fail();

                    var resBytes = (await res.Content.ReadAsByteArrayAsync()).Length;
                    return Response.Ok(sizeBytes: reqBytes + resBytes);
                }
                catch
                {
                    return Response.Fail();
                }
            })
            .WithLoadSimulations(
                // 100 requests per second, gedurende 1 minuut, met interval van 1s 
                Simulation.Inject(
                    rate: 100,
                    during: TimeSpan.FromMinutes(1),
                    interval: TimeSpan.FromSeconds(1)
                )
            );
        }
    }
}
