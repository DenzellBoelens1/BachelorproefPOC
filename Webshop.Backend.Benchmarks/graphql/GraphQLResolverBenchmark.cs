// File: GraphQLResolverBenchmark.cs
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Microsoft.EntityFrameworkCore;
using Webshop.Backend.Data;      // jouw AppDbContext
using Webshop.Backend.Services;  // ProductService
using Webshop.Shared.Models;     // Product & ProductOption

namespace Webshop.Backend.Benchmarks.graphql
{
    [MemoryDiagnoser]
    public class GraphQLResolverBenchmark
    {
        private ProductService _service;
        private HttpClient _client;

        [Params(1, 10, 50)]
        public int ProductId { get; set; }

        [GlobalSetup]
        public void Setup()
        {
            // 1) DbContextOptions met één InMemory database
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase("BenchDb")
                .Options;

            // 2) Seed-context: vul de database maar één keer
            using (var seedContext = new AppDbContext(options))
            {
                // Maak ‘m schoon
                seedContext.Products.RemoveRange(seedContext.Products);
                seedContext.SaveChanges();

                int globalOptionId = 1;
                for (int i = 1; i <= 100; i++)
                {
                    var p = new Product
                    {
                        ProductID = i,
                        Name = $"P{i}",
                        Description = $"Seeded product {i}",
                        InStock = 100,         // nu ingevuld
                        MinStock = 0,           // nu ingevuld
                        BasePrice = 9.99m        // nu ingevuld
                    };

                    p.Options = Enumerable.Range(1, 5)
                        .Select(_ => new ProductOption
                        {
                            OptionID = globalOptionId++,
                            ProductID = i,
                            OptionValue = "X"
                        })
                        .ToList();

                    seedContext.Products.Add(p);
                }
                seedContext.SaveChanges();
            }

            // 3) Service‐context: nieuwe context op dezelfde InMemory DB
            var serviceContext = new AppDbContext(options);
            _service = new ProductService(serviceContext);

            // 4) GraphQL‐endpoint client
            _client = new HttpClient
            {
                BaseAddress = new Uri("http://localhost:5139")
            };
        }

        [Benchmark(Description = "Direct Service Call")]
        public async Task DirectService()
        {
            var dto = await _service.GetProductDetailsAsync(ProductId);
            _ = dto.Options.Count;
        }

        [Benchmark(Description = "GraphQL HTTP Call")]
        public async Task GraphQLHttp()
        {
            var payload = new
            {
                query = @"query($id:Int!){
                            productDetails(id:$id){
                                productID
                                name
                                options { optionID }
                            }
                         }",
                variables = new { id = ProductId }
            };

            var res = await _client.PostAsJsonAsync("/graphql", payload);
            res.EnsureSuccessStatusCode();
            _ = await res.Content.ReadAsStringAsync();
        }

        // entry-point removed: draait via BenchmarkSwitcher in Program.cs
    }
}
