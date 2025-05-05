// File: ProductHubTests.cs
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Moq;
using Webshop.Backend.Data;
using Webshop.Backend.Hubs;
using Webshop.Backend.Services;
using Webshop.Shared.DTOs;
using Webshop.Shared.Models;
using Xunit;

namespace Webshop.Backend.Tests.Hubs
{
    public class ProductHubTests
    {
        private static void InjectClients(Hub hub, IHubCallerClients clients)
        {
            var prop = typeof(Hub).GetProperty(
                nameof(Hub.Clients),
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
            );
            prop!.SetValue(hub, clients);
        }

        private static ProductService CreateServiceWithEmptyDb()
        {
            var opts = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase("EmptyDb_" + Guid.NewGuid())
                .Options;
            return new ProductService(new AppDbContext(opts));
        }

        private static ProductService CreateServiceWithOnePricedProduct()
        {
            var opts = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase("PricedDb_" + Guid.NewGuid())
                .Options;
            var ctx = new AppDbContext(opts);
            ctx.Products.Add(new Product
            {
                ProductID = 1,
                Name = "Test",
                Description = "D",
                InStock = 100,
                MinStock = 0,
                BasePrice = 10m,
                Options = new List<ProductOption>()
            });
            ctx.SaveChanges();
            return new ProductService(ctx);
        }

        [Fact]
        public async Task UpdateStock_ProductNotFound_SendsProductNotFound()
        {
            // Arrange
            var service = CreateServiceWithEmptyDb();
            var hub = new ProductHub(service);

            // Hier vangen we in deze variabelen wat er wordt opgeroepen:
            string calledMethod = null!;
            object[] calledArgs = null!;

            // Mock ISingleClientProxy
            var callerMock = new Mock<ISingleClientProxy>();
            callerMock
                .Setup(x => x.SendCoreAsync(
                    It.IsAny<string>(),
                    It.IsAny<object[]>(),
                    It.IsAny<CancellationToken>()))
                .Callback<string, object[], CancellationToken>((m, a, _) =>
                {
                    calledMethod = m;
                    calledArgs = a;
                })
                .Returns(Task.CompletedTask);

            var clientsMock = new Mock<IHubCallerClients>();
            clientsMock.Setup(c => c.Caller).Returns(callerMock.Object);
            InjectClients(hub, clientsMock.Object);

            // Act
            await hub.UpdateStock(42, 5);

            // Assert
            Assert.Equal("ProductNotFound", calledMethod);
            Assert.Single(calledArgs);
            Assert.Equal(42, (int)calledArgs[0]);
        }

        [Fact]
        public async Task CalculatePrice_ValidRequest_SendsReceivePriceCalculation()
        {
            // Arrange
            var service = CreateServiceWithOnePricedProduct();
            var hub = new ProductHub(service);

            string calledMethod = null!;
            object[] calledArgs = null!;

            var callerMock = new Mock<ISingleClientProxy>();
            callerMock
                .Setup(x => x.SendCoreAsync(
                    It.IsAny<string>(),
                    It.IsAny<object[]>(),
                    It.IsAny<CancellationToken>()))
                .Callback<string, object[], CancellationToken>((m, a, _) =>
                {
                    calledMethod = m;
                    calledArgs = a;
                })
                .Returns(Task.CompletedTask);

            var clientsMock = new Mock<IHubCallerClients>();
            clientsMock.Setup(c => c.Caller).Returns(callerMock.Object);
            InjectClients(hub, clientsMock.Object);

            // Act: quantity = 3 à unitPrice = 10 ⇒ totalPrice = 30
            await hub.CalculatePrice(
                productId: 1,
                quantity: 3,
                selectedOptionIds: new List<int>(),
                optionValues: new Dictionary<int, string>(),
                customText: null
            );

            // Assert
            Assert.Equal("ReceivePriceCalculation", calledMethod);
            Assert.Single(calledArgs);
            var dto = Assert.IsType<PriceDTO>(calledArgs[0]);
            Assert.Equal(10m, dto.UnitPrice);
            Assert.Equal(30m, dto.TotalPrice);
        }
    }
}
