```

BenchmarkDotNet v0.14.0, Windows 11 (10.0.26100.3775)
12th Gen Intel Core i7-12700H, 1 CPU, 20 logical and 14 physical cores
.NET SDK 8.0.400
  [Host]     : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2


```
| Method                                | ProductId | Mean     | Error     | StdDev    | Allocated |
|-------------------------------------- |---------- |---------:|----------:|----------:|----------:|
| **&#39;REST GET /api/products/details/{id}&#39;** | **1**         | **4.977 ms** | **0.0766 ms** | **0.0716 ms** |   **4.93 KB** |
| **&#39;REST GET /api/products/details/{id}&#39;** | **50**        | **4.763 ms** | **0.0851 ms** | **0.0796 ms** |      **5 KB** |
| **&#39;REST GET /api/products/details/{id}&#39;** | **100**       | **4.779 ms** | **0.0593 ms** | **0.0526 ms** |   **5.05 KB** |
