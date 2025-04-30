```

BenchmarkDotNet v0.14.0, Windows 11 (10.0.26100.3775)
12th Gen Intel Core i7-12700H, 1 CPU, 20 logical and 14 physical cores
.NET SDK 8.0.400
  [Host]     : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2


```
| Method                     | ConcurrentRequests | Mean | Error |
|--------------------------- |------------------- |-----:|------:|
| **&#39;REST CalculatePrice&#39;**      | **50**                 |   **NA** |    **NA** |
| &#39;GraphQL CalculatePrice&#39;   | 50                 |   NA |    NA |
| &#39;SignalR CalculatePrice&#39;   | 50                 |   NA |    NA |
| &#39;WebSocket CalculatePrice&#39; | 50                 |   NA |    NA |
| **&#39;REST CalculatePrice&#39;**      | **100**                |   **NA** |    **NA** |
| &#39;GraphQL CalculatePrice&#39;   | 100                |   NA |    NA |
| &#39;SignalR CalculatePrice&#39;   | 100                |   NA |    NA |
| &#39;WebSocket CalculatePrice&#39; | 100                |   NA |    NA |
| **&#39;REST CalculatePrice&#39;**      | **200**                |   **NA** |    **NA** |
| &#39;GraphQL CalculatePrice&#39;   | 200                |   NA |    NA |
| &#39;SignalR CalculatePrice&#39;   | 200                |   NA |    NA |
| &#39;WebSocket CalculatePrice&#39; | 200                |   NA |    NA |

Benchmarks with issues:
  PriceBenchmark.'REST CalculatePrice': DefaultJob [ConcurrentRequests=50]
  PriceBenchmark.'GraphQL CalculatePrice': DefaultJob [ConcurrentRequests=50]
  PriceBenchmark.'SignalR CalculatePrice': DefaultJob [ConcurrentRequests=50]
  PriceBenchmark.'WebSocket CalculatePrice': DefaultJob [ConcurrentRequests=50]
  PriceBenchmark.'REST CalculatePrice': DefaultJob [ConcurrentRequests=100]
  PriceBenchmark.'GraphQL CalculatePrice': DefaultJob [ConcurrentRequests=100]
  PriceBenchmark.'SignalR CalculatePrice': DefaultJob [ConcurrentRequests=100]
  PriceBenchmark.'WebSocket CalculatePrice': DefaultJob [ConcurrentRequests=100]
  PriceBenchmark.'REST CalculatePrice': DefaultJob [ConcurrentRequests=200]
  PriceBenchmark.'GraphQL CalculatePrice': DefaultJob [ConcurrentRequests=200]
  PriceBenchmark.'SignalR CalculatePrice': DefaultJob [ConcurrentRequests=200]
  PriceBenchmark.'WebSocket CalculatePrice': DefaultJob [ConcurrentRequests=200]
