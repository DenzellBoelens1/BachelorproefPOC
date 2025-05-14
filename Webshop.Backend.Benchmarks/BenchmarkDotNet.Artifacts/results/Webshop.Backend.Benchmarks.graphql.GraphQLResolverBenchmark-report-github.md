```

BenchmarkDotNet v0.14.0, Windows 11 (10.0.26100.3775)
12th Gen Intel Core i7-12700H, 1 CPU, 20 logical and 14 physical cores
.NET SDK 8.0.400
  [Host]     : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2


```
| Method                | ProductId | Mean        | Error     | StdDev     | Gen0   | Gen1   | Allocated |
|---------------------- |---------- |------------:|----------:|-----------:|-------:|-------:|----------:|
| **&#39;Direct Service Call&#39;** | **1**         |    **60.74 μs** |  **1.151 μs** |   **0.961 μs** | **7.3242** | **0.7324** |   **90.4 KB** |
| &#39;GraphQL HTTP Call&#39;   | 1         | 2,696.87 μs | 51.181 μs |  60.927 μs |      - |      - |   4.85 KB |
| **&#39;Direct Service Call&#39;** | **10**        |    **60.79 μs** |  **1.195 μs** |   **1.173 μs** | **7.3242** | **0.9766** |  **90.46 KB** |
| &#39;GraphQL HTTP Call&#39;   | 10        | 2,707.43 μs | 53.520 μs |  97.865 μs |      - |      - |   4.81 KB |
| **&#39;Direct Service Call&#39;** | **50**        |    **61.44 μs** |  **0.530 μs** |   **0.442 μs** | **7.3242** | **0.7324** |  **90.32 KB** |
| &#39;GraphQL HTTP Call&#39;   | 50        | 2,730.86 μs | 53.819 μs | 119.259 μs |      - |      - |   4.83 KB |
