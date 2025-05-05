```

BenchmarkDotNet v0.14.0, Windows 11 (10.0.26100.3775)
12th Gen Intel Core i7-12700H, 1 CPU, 20 logical and 14 physical cores
.NET SDK 8.0.400
  [Host]     : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2


```
| Method                | ProductId | Mean        | Error     | StdDev    | Gen0   | Gen1   | Allocated |
|---------------------- |---------- |------------:|----------:|----------:|-------:|-------:|----------:|
| **&#39;Direct Service Call&#39;** | **1**         |    **26.12 μs** |  **0.501 μs** |  **0.597 μs** | **3.7842** | **0.3662** |  **46.88 KB** |
| &#39;GraphQL HTTP Call&#39;   | 1         | 3,008.52 μs | 58.752 μs | 87.938 μs |      - |      - |    4.9 KB |
| **&#39;Direct Service Call&#39;** | **10**        |    **25.31 μs** |  **0.217 μs** |  **0.203 μs** | **3.7842** | **0.3662** |  **46.88 KB** |
| &#39;GraphQL HTTP Call&#39;   | 10        | 2,933.21 μs | 54.680 μs | 69.152 μs |      - |      - |   4.91 KB |
| **&#39;Direct Service Call&#39;** | **50**        |    **24.53 μs** |  **0.408 μs** |  **0.341 μs** | **3.7842** | **0.3662** |  **46.88 KB** |
| &#39;GraphQL HTTP Call&#39;   | 50        | 2,952.24 μs | 58.764 μs | 80.436 μs |      - |      - |   4.85 KB |
