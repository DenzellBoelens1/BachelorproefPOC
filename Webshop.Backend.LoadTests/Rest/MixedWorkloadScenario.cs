// File: MixedWorkloadScenario.cs
using System;
using System.Net.Http;
using System.Net.Http.Json;
using NBomber.CSharp;
using NBomber.Contracts;

namespace Webshop.Backend.LoadTests
{
    public static class MixedWorkloadScenario
    {
        private static readonly HttpClient _client = new HttpClient
        {
            BaseAddress = new Uri("http://localhost:5139")
        };

        /// <summary>
        /// Read‐scenario: 70 rps steady + burst tot 90 rps voor 30s
        /// </summary>
        public static ScenarioProps CreateReadScenario() =>
            Scenario.Create("mixed_reads", async context =>
            {
                var res = await _client.GetAsync("/api/products?page=1&pageSize=10");
                return res.IsSuccessStatusCode
                    ? Response.Ok(sizeBytes: (await res.Content.ReadAsByteArrayAsync()).Length)
                    : Response.Fail();
            })
            .WithLoadSimulations(
                // 70 rps gedurende 60s
                Simulation.Inject(rate: 70, during: TimeSpan.FromMinutes(1), interval: TimeSpan.FromSeconds(1)),
                // burst tot 90 rps gedurende 30s
                Simulation.Inject(rate: 90, during: TimeSpan.FromSeconds(30), interval: TimeSpan.FromSeconds(1))
            );

        /// <summary>
        /// Write‐scenario: 30 rps steady + burst tot 40 rps voor 30s
        /// </summary>
        public static ScenarioProps CreateWriteScenario() =>
            Scenario.Create("mixed_writes", async context =>
            {
                var rnd = new Random();
                var payload = new { ProductID = rnd.Next(1, 101), InStock = rnd.Next(0, 500) };
                var res = await _client.PutAsJsonAsync($"/api/products/{payload.ProductID}/stock", payload);
                return res.IsSuccessStatusCode
                    ? Response.Ok(sizeBytes: (await res.Content.ReadAsByteArrayAsync()).Length)
                    : Response.Fail();
            })
            .WithLoadSimulations(
                // 30 rps gedurende 60s
                Simulation.Inject(rate: 30, during: TimeSpan.FromMinutes(1), interval: TimeSpan.FromSeconds(1)),
                // burst tot 40 rps gedurende 30s
                Simulation.Inject(rate: 40, during: TimeSpan.FromSeconds(30), interval: TimeSpan.FromSeconds(1))
            );
    }
}
