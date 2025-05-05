// File: Program.cs
using BenchmarkDotNet.Running;

namespace Webshop.Backend.Benchmarks
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Pakt alle types met [Benchmark] in deze assembly op en draait ze
            BenchmarkSwitcher
                .FromAssembly(typeof(Program).Assembly)
                .Run(args);
        }
    }
}
