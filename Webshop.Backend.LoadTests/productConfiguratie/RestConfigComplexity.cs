// RestConfigComplexity.cs
using System;
using System.Net.Http;
using NBomber.CSharp;
using NBomber.Contracts;

namespace Webshop.Backend.LoadTests
{
    public static class RestConfigComplexity
    {
        private static readonly HttpClient client =
            new HttpClient { BaseAddress = new Uri("http://localhost:5139") };

        /// <summary>
        /// optionAliases = aantal extra opties om mee te geven
        /// includeOptions = of we de options-parameter aanzetten
        /// </summary>
        public static ScenarioProps CreateScenario(int optionAliases, bool includeOptions)
        {
            var rnd = new Random();
            var scenarioName = $"rest_details_opts{optionAliases}_incl{includeOptions}";

            return Scenario.Create(scenarioName, async context =>
            {
                int productId = rnd.Next(1, 101);

                // Bouw de query-string
                var qs = $"?includeOptions={includeOptions.ToString().ToLower()}" +
                         $"&optionAliases={optionAliases}";
                var url = $"/api/products/details/{productId}{qs}";

                try
                {
                    var res = await client.GetAsync(url);

                    // bij elke niet-2xx status gewoon een Fail zonder argumenten
                    if (!res.IsSuccessStatusCode)
                        return Response.Fail();

                    var bytes = await res.Content.ReadAsByteArrayAsync();
                    // geef de grootte van de payload door aan NBomber
                    return Response.Ok(sizeBytes: bytes.Length);
                }
                catch
                {
                    // bij exception ook alleen Fail()
                    return Response.Fail();
                }
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
