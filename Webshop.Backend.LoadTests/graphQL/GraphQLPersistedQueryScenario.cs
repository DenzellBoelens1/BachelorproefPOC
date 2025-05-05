// File: GraphQLPersistedQueryScenario.cs
using System;
using System.Net.Http;
using System.Net.Http.Json;
using NBomber.CSharp;
using NBomber.Contracts;

namespace Webshop.Backend.LoadTests
{
    public static class GraphQLPersistedQueryScenario
    {
        private static readonly HttpClient _client = new HttpClient
        {
            BaseAddress = new Uri("http://localhost:5139")
        };

        public static ScenarioProps CreateInlineQueryScenario()
        {
            var inlineQuery = @"
                query GetFullProduct($id:Int!){
                  productDetails(id:$id){
                    productID
                    name
                    inStock
                    options { optionID, optionValue }
                  }
                }";
            return Scenario.Create("graphql_inline_query", async context =>
            {
                var res = await _client.PostAsJsonAsync("/graphql", new
                {
                    query = inlineQuery,
                    variables = new { id = 42 }
                });
                return res.IsSuccessStatusCode
                    ? Response.Ok(sizeBytes: (await res.Content.ReadAsByteArrayAsync()).Length)
                    : Response.Fail();
            })
            .WithLoadSimulations(
                Simulation.Inject(rate: 20, during: TimeSpan.FromMinutes(1), interval: TimeSpan.FromSeconds(1))
            );
        }

        //public static ScenarioProps CreatePersistedQueryScenario()
        //{
        //    // veronderstel dat je server een persisted query map heeft
        //    const string queryId = "GetFullProduct";
        //    return Scenario.Create("graphql_persisted_query", async context =>
        //    {
        //        var res = await _client.PostAsJsonAsync("/graphql", new
        //        {
        //            id = queryId,
        //            variables = new { id = 42 }
        //        });
        //        return res.IsSuccessStatusCode
        //            ? Response.Ok(sizeBytes: (await res.Content.ReadAsByteArrayAsync()).Length)
        //            : Response.Fail();
        //    })
        //    .WithLoadSimulations(
        //        Simulation.Inject(rate: 20, during: TimeSpan.FromMinutes(1), interval: TimeSpan.FromSeconds(1))
        //    );
        //}
    }
}
