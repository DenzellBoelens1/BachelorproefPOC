```

BenchmarkDotNet v0.14.0, Windows 11 (10.0.26100.3775)
12th Gen Intel Core i7-12700H, 1 CPU, 20 logical and 14 physical cores
.NET SDK 8.0.400
  [Host]     : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2


```
| Method                     | ConcurrentRequests | Mean       | Error     | StdDev    | Median     | Gen0     | Gen1     | Allocated  |
|--------------------------- |------------------- |-----------:|----------:|----------:|-----------:|---------:|---------:|-----------:|
| **&#39;REST CalculatePrice&#39;**      | **50**                 |   **1.190 ms** | **0.0468 ms** | **0.1364 ms** |   **1.237 ms** |  **11.7188** |        **-** |  **171.04 KB** |
| &#39;GraphQL CalculatePrice&#39;   | 50                 |   1.641 ms | 0.0454 ms | 0.1317 ms |   1.650 ms |  15.6250 |        - |  198.31 KB |
| &#39;SignalR CalculatePrice&#39;   | 50                 | 128.823 ms | 2.5014 ms | 3.9675 ms | 129.126 ms |        - |        - |  168.16 KB |
| &#39;WebSocket CalculatePrice&#39; | 50                 |  32.579 ms | 0.5555 ms | 0.4639 ms |  32.772 ms |  83.3333 |        - | 1227.62 KB |
| **&#39;REST CalculatePrice&#39;**      | **100**                |   **2.146 ms** | **0.0601 ms** | **0.1771 ms** |   **2.199 ms** |  **27.3438** |        **-** |  **342.01 KB** |
| &#39;GraphQL CalculatePrice&#39;   | 100                |   3.114 ms | 0.0614 ms | 0.1348 ms |   3.122 ms |  31.2500 |        - |  409.84 KB |
| &#39;SignalR CalculatePrice&#39;   | 100                | 251.172 ms | 4.9541 ms | 8.8059 ms | 248.103 ms |        - |        - |  333.09 KB |
| &#39;WebSocket CalculatePrice&#39; | 100                |  64.259 ms | 1.2414 ms | 1.6572 ms |  64.738 ms | 250.0000 | 125.0000 | 2427.24 KB |
| **&#39;REST CalculatePrice&#39;**      | **200**                |   **4.213 ms** | **0.0851 ms** | **0.2484 ms** |   **4.178 ms** |  **54.6875** |   **3.9063** |  **683.21 KB** |
| &#39;GraphQL CalculatePrice&#39;   | 200                |   6.114 ms | 0.1197 ms | 0.1556 ms |   6.043 ms |  62.5000 |   7.8125 |   787.8 KB |
| &#39;SignalR CalculatePrice&#39;   | 200                | 502.021 ms | 9.8662 ms | 9.2288 ms | 504.668 ms |        - |        - |  665.35 KB |
| &#39;WebSocket CalculatePrice&#39; | 200                | 123.016 ms | 1.5330 ms | 1.3590 ms | 122.999 ms | 400.0000 | 200.0000 | 4840.31 KB |
