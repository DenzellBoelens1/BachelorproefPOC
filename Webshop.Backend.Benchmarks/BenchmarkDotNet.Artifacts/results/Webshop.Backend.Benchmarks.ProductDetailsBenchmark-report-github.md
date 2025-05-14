```

BenchmarkDotNet v0.14.0, Windows 11 (10.0.26100.3775)
12th Gen Intel Core i7-12700H, 1 CPU, 20 logical and 14 physical cores
.NET SDK 8.0.400
  [Host]     : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2


```
| Method                                | ProductId | Mean     | Error     | StdDev    | Allocated |
|-------------------------------------- |---------- |---------:|----------:|----------:|----------:|
| **&#39;REST GET /api/products/details/{id}&#39;** | **1**         | **2.764 ms** | **0.0525 ms** | **0.0645 ms** |   **4.94 KB** |
| **&#39;REST GET /api/products/details/{id}&#39;** | **50**        | **2.675 ms** | **0.0531 ms** | **0.0709 ms** |      **5 KB** |
| **&#39;REST GET /api/products/details/{id}&#39;** | **100**       | **2.689 ms** | **0.0531 ms** | **0.0709 ms** |   **5.05 KB** |
