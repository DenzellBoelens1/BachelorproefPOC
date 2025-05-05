// File: ProductDetailsBenchmark.cs
using System;
using System.Net.Http;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace Webshop.Backend.Benchmarks
{
    [MemoryDiagnoser]
    public class ProductDetailsBenchmark
    {
        private readonly Uri _baseUrl = new Uri("http://localhost:5139");
        private HttpClient _httpClient = default!;

        // varieer hier op verschillende product-IDs om te zien of grootte/complexiteit impact heeft
        [Params(1, 50, 100)]
        public int ProductId { get; set; }

        [GlobalSetup]
        public void Setup()
        {
            // connection pooling instellen, net als in PriceBenchmark
            var handler = new SocketsHttpHandler
            {
                MaxConnectionsPerServer = 100,
                PooledConnectionLifetime = TimeSpan.FromSeconds(30),
                PooledConnectionIdleTimeout = TimeSpan.FromSeconds(15)
            };
            _httpClient = new HttpClient(handler)
            {
                BaseAddress = _baseUrl,
                Timeout = TimeSpan.FromSeconds(10)
            };
        }

        [Benchmark(Description = "REST GET /api/products/details/{id}")]
        public async Task<string> GetProductDetails()
        {
            // haal het JSON terug en return de inhoud (latency én allocaties worden gemeten)
            var res = await _httpClient.GetAsync($"/api/products/details/{ProductId}");
            res.EnsureSuccessStatusCode();
            return await res.Content.ReadAsStringAsync();
        }

        // Optioneel: opruimen
        [GlobalCleanup]
        public void Cleanup()
        {
            _httpClient.Dispose();
        }

        
    }
}
