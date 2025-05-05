// File: ProductServiceTests.cs
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Webshop.Backend.Data;
using Webshop.Backend.Services;
using Webshop.Shared.Models;
using Xunit;

namespace Webshop.Backend.Tests
{
    public class ProductServiceTests
    {
        [Fact]
        public async Task GetProductDetailsAsync_LoadsAllOptions_InSingleQuery()
        {
            // --- 1) Open een in-memory SQLite connection
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            // --- 2) Maak een LoggerFactory en hook onze custom provider
            int queryCount = 0;
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddProvider(new QueryCountingLoggerProvider(_ => queryCount++));
            });

            // --- 3) Bouw DbContextOptions met logging
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(connection)
                .UseLoggerFactory(loggerFactory)
                .EnableSensitiveDataLogging()
                .Options;

            // --- 4) Maak én seed de database
            await using (var dbInit = new AppDbContext(options))
            {
                dbInit.Database.EnsureCreated();

                var product = new Product
                {
                    ProductID = 1,
                    Name = "Test",
                    Description = "D",
                    InStock = 42,
                    MinStock = 1,
                    BasePrice = 9.99m,
                    Options = Enumerable.Range(1, 10)
                        .Select(i => new ProductOption
                        {
                            OptionID = i,
                            ProductID = 1,
                            OptionValue = $"V{i}"
                        })
                        .ToList()
                };

                dbInit.Products.Add(product);
                await dbInit.SaveChangesAsync();
            }

            // --- 5) Reset de teller vóór de échte service-aanroep
            queryCount = 0;

            // --- 6) Roep je service aan
            await using var db = new AppDbContext(options);
            var service = new ProductService(db);
            var dto = await service.GetProductDetailsAsync(1);

            // --- 7) Asserties
            Assert.Equal(10, dto.Options.Count); // alle 10 opties terug
            Assert.Equal(1, queryCount);         // precies 1 SQL-query
        }
    }

    // LoggerProvider om EF Core SQL-calls te tellen
    internal class QueryCountingLoggerProvider : ILoggerProvider
    {
        private readonly Action<string> _onLog;
        public QueryCountingLoggerProvider(Action<string> onLog) => _onLog = onLog;
        public ILogger CreateLogger(string categoryName) =>
            new QueryCountingLogger(_onLog, categoryName);
        public void Dispose() { }

        private class QueryCountingLogger : ILogger
        {
            private readonly Action<string> _onLog;
            private readonly string _category;
            public QueryCountingLogger(Action<string> onLog, string category)
            {
                _onLog = onLog;
                _category = category;
            }

            public IDisposable BeginScope<TState>(TState state) => NullScope.Instance;
            public bool IsEnabled(LogLevel level) => true;

            public void Log<TState>(
                LogLevel level,
                EventId id,
                TState state,
                Exception? exception,
                Func<TState, Exception?, string> formatter)
            {
                // EF Core SQL category = "Microsoft.EntityFrameworkCore.Database.Command"
                if (_category.Contains("Database.Command") &&
                    level == LogLevel.Information)
                {
                    _onLog(formatter(state, exception));
                }
            }
        }

        private class NullScope : IDisposable
        {
            public static NullScope Instance { get; } = new NullScope();
            public void Dispose() { }
        }
    }
}
