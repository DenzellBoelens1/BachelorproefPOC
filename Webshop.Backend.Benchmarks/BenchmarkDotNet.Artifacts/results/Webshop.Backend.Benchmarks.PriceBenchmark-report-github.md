```

BenchmarkDotNet v0.14.0, Windows 11 (10.0.26100.3775)
12th Gen Intel Core i7-12700H, 1 CPU, 20 logical and 14 physical cores
.NET SDK 8.0.400
  [Host]     : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2


```
| Method                     | ConcurrentRequests | Mean         | Error      | StdDev     | Gen0     | Gen1    | Allocated  |
|--------------------------- |------------------- |-------------:|-----------:|-----------:|---------:|--------:|-----------:|
| **&#39;REST CalculatePrice&#39;**      | **50**                 |     **1.803 ms** |  **0.0204 ms** |  **0.0171 ms** |   **7.8125** |       **-** |  **171.18 KB** |
| &#39;GraphQL CalculatePrice&#39;   | 50                 |     2.683 ms |  0.0536 ms |  0.0819 ms |  15.6250 |       - |  195.05 KB |
| &#39;SignalR CalculatePrice&#39;   | 50                 |   267.083 ms |  5.2134 ms |  5.3537 ms |        - |       - |  173.43 KB |
| &#39;WebSocket CalculatePrice&#39; | 50                 |    56.344 ms |  0.9574 ms |  0.8955 ms | 111.1111 |       - | 1245.32 KB |
| **&#39;REST CalculatePrice&#39;**      | **100**                |     **3.348 ms** |  **0.0516 ms** |  **0.0482 ms** |  **27.3438** |       **-** |  **341.86 KB** |
| &#39;GraphQL CalculatePrice&#39;   | 100                |     4.952 ms |  0.0974 ms |  0.1121 ms |  31.2500 |       - |  389.88 KB |
| &#39;SignalR CalculatePrice&#39;   | 100                |   565.148 ms | 11.1875 ms | 11.9705 ms |        - |       - |  352.39 KB |
| &#39;WebSocket CalculatePrice&#39; | 100                |   106.683 ms |  2.1028 ms |  2.5824 ms | 200.0000 |       - | 2469.58 KB |
| **&#39;REST CalculatePrice&#39;**      | **200**                |     **6.474 ms** |  **0.0846 ms** |  **0.0792 ms** |  **54.6875** |       **-** |  **683.32 KB** |
| &#39;GraphQL CalculatePrice&#39;   | 200                |     9.586 ms |  0.1901 ms |  0.1952 ms |  62.5000 | 15.6250 |  778.56 KB |
| &#39;SignalR CalculatePrice&#39;   | 200                | 1,169.264 ms | 23.1315 ms | 25.7106 ms |        - |       - |  664.55 KB |
| &#39;WebSocket CalculatePrice&#39; | 200                |   201.697 ms |  3.9771 ms |  6.6449 ms | 500.0000 |       - | 4888.98 KB |
