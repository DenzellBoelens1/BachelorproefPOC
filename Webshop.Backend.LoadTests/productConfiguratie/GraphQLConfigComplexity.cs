// GraphQLConfigComplexity.cs
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using NBomber.CSharp;
using NBomber.Contracts;

namespace Webshop.Backend.LoadTests
{
    public static class GraphQLConfigComplexity
    {
        private static readonly HttpClient client =
            new HttpClient { BaseAddress = new Uri("http://localhost:5139") };

        /// <summary>
        /// optionAliases = aantal keren dat we de 'options' alias toevoegen
        /// includeOptions = zet de hele options-sectie aan of uit
        /// </summary>
        public static ScenarioProps CreateScenario(int optionAliases, bool includeOptions)
        {
            // bouw de selectie van de options-sectie
            var optionsSection = new StringBuilder();
            if (includeOptions)
            {
                for (int i = 0; i < optionAliases; i++)
                {
                    optionsSection.AppendLine($@"
                        optionsAlias{i}: options {{
                          optionID
                          optionType
                          optionValue
                        }}");
                }
            }

            // stel de volledige selectie-string samen
            var selectionSet = $@"
                productID
                name
                inStock
                basePrice
                {optionsSection}";

            // scenario-naam bevat parameters zodat je ze makkelijk uit elkaar houdt
            var scenarioName = $"graphql_details_opts{optionAliases}_incl{includeOptions}";

            return Scenario.Create(scenarioName, async context =>
            {
                var rnd = new Random();
                int id = rnd.Next(1, 101);

                var payload = new
                {
                    query = $@"
                        query GetDetails($id:Int!) {{
                          productDetails(id:$id) {{
                            {selectionSet}
                          }}
                        }}",
                    variables = new { id }
                };

                var res = await client.PostAsJsonAsync("/graphql", payload);

                // als de request mislukt, return gewoon de Fail() overload
                if (!res.IsSuccessStatusCode)
                    return Response.Fail();

                // anders meet je de size van de body
                var bytes = await res.Content.ReadAsByteArrayAsync();
                return Response.Ok(sizeBytes: bytes.Length);
            })
            .WithLoadSimulations(
                // 50 requests per seconde, 1s interval, gedurende 1 minuut
                Simulation.Inject(
                    rate: 50,
                    during: TimeSpan.FromMinutes(1),
                    interval: TimeSpan.FromSeconds(1)
                )
            );
        }
    }
}
