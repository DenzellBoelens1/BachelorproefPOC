// GraphQLStockUpdate.cs
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using NBomber.CSharp;
using NBomber.Contracts;

namespace Webshop.Backend.LoadTests
{
    public static class GraphQLStockUpdate
    {
        // Hergebruik een statische HttpClient voor connectie-efficiëntie
        private static readonly HttpClient client = new HttpClient { BaseAddress = new Uri("http://localhost:5139") };

        public static ScenarioProps CreateScenario()
        {
            var rnd = new Random();
            return Scenario.Create("graphql_stock_update", async context =>
            {
                int id = rnd.Next(1, 101);
                int stock = rnd.Next(0, 501);

                var payload = new
                {
                    query = @"
                        mutation UpdateStock($id:Int!,$stock:Int!){
                          updateProductStock(productID:$id,inStock:$stock){
                            productID
                            inStock
                          }
                        }",
                    variables = new { id, stock }
                };

                try
                {
                    var res = await client.PostAsJsonAsync("/graphql", payload);
                    if (!res.IsSuccessStatusCode)
                        return Response.Fail();

                    // Meet de grootte van de response body voor datatransfer
                    var bytes = await res.Content.ReadAsByteArrayAsync();
                    return Response.Ok(sizeBytes: bytes.Length);
                }
                catch
                {
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