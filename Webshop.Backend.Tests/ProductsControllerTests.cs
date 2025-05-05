// File: ProductsControllerTests.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Webshop.Backend.Controllers;
using Webshop.Backend.Data;
using Webshop.Backend.Services;
using Webshop.Shared.DTOs;
using Webshop.Shared.Models;
using Xunit;

namespace Webshop.Backend.Tests
{
    public class ProductsControllerTests
    {
        private AppDbContext CreateContextWithSeedData()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var ctx = new AppDbContext(options);

            // seed 2 producten
            ctx.Products.AddRange(new[]
            {
                new Product
                {
                    ProductID = 1,
                    Name = "Alpha",
                    InStock = 10,
                    MinStock = 0,
                    BasePrice = 1.0m,
                    Description = "Desc A"
                },
                new Product
                {
                    ProductID = 2,
                    Name = "Beta",
                    InStock = 20,
                    MinStock = 0,
                    BasePrice = 2.0m,
                    Description = "Desc B"
                }
            });
            ctx.SaveChanges();
            return ctx;
        }

        [Fact]
        public async Task GetProducts_ReturnsOk_WithListOfIndexes()
        {
            // arrange
            var ctx = CreateContextWithSeedData();
            var svc = new ProductService(ctx);
            var ctrl = new ProductsController(svc);

            // act
            var result = await ctrl.GetProducts(page: 1, pageSize: 10, search: null);

            // assert
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var list = Assert.IsAssignableFrom<IEnumerable<ProductDTO.Index>>(ok.Value);
            Assert.Equal(2, list.Count());
        }

        [Fact]
        public async Task GetProduct_ExistingId_ReturnsOk()
        {
            // arrange
            var ctx = CreateContextWithSeedData();
            var svc = new ProductService(ctx);
            var ctrl = new ProductsController(svc);

            // act
            var result = await ctrl.GetProduct(1);

            // assert
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var dto = Assert.IsType<ProductDTO.Index>(ok.Value);
            Assert.Equal(1, dto.ProductID);
            Assert.Equal("Alpha", dto.Name);
        }

        [Fact]
        public async Task GetProduct_NonExistingId_ReturnsNotFound()
        {
            // arrange
            var ctx = CreateContextWithSeedData();
            var svc = new ProductService(ctx);
            var ctrl = new ProductsController(svc);

            // act
            var result = await ctrl.GetProduct(999);

            // assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task GetProductDetails_ExistingId_ReturnsOk()
        {
            // arrange: seed ook opties mee
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            await using var ctxSeed = new AppDbContext(options);
            var p = new Product { ProductID = 42, Name = "X", InStock = 5, MinStock = 0, BasePrice = 1m, Description = "D" };
            p.Options.AddRange(Enumerable.Range(1, 3)
                .Select(i => new ProductOption
                {
                    OptionID = i,
                    ProductID = 42,
                    OptionValue = $"O{i}",
                    OptionType = "T"
                }));
            ctxSeed.Products.Add(p);
            await ctxSeed.SaveChangesAsync();

            var svc = new ProductService(ctxSeed);
            var ctrl = new ProductsController(svc);

            // act
            var result = await ctrl.GetProductDetails(42);

            // assert
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var dto = Assert.IsType<ProductDTO.Details>(ok.Value);
            Assert.Equal(42, dto.ProductID);
            Assert.Equal(3, dto.Options.Count);
        }

        [Fact]
        public async Task GetProductDetails_NonExistingId_ReturnsNotFound()
        {
            var ctx = CreateContextWithSeedData();
            var svc = new ProductService(ctx);
            var ctrl = new ProductsController(svc);

            var result = await ctrl.GetProductDetails(777);
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task UpdateStock_ExistingId_ReturnsOkWithUpdatedIndex()
        {
            // arrange
            var ctx = CreateContextWithSeedData();
            var svc = new ProductService(ctx);
            var ctrl = new ProductsController(svc);

            var dtoIn = new ProductDTO.UpdateStock { ProductID = 1, InStock = 123 };

            // act
            var result = await ctrl.UpdateStock(1, dtoIn);

            // assert
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var dtoOut = Assert.IsType<ProductDTO.Index>(ok.Value);
            Assert.Equal(1, dtoOut.ProductID);
            Assert.Equal(123, dtoOut.InStock);
        }

        [Fact]
        public async Task UpdateStock_NonExistingId_ReturnsNotFound()
        {
            var ctx = CreateContextWithSeedData();
            var svc = new ProductService(ctx);
            var ctrl = new ProductsController(svc);

            var dtoIn = new ProductDTO.UpdateStock { ProductID = 999, InStock = 5 };

            var result = await ctrl.UpdateStock(999, dtoIn);
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task CalculatePrice_ReturnsOkWithPriceDTO()
        {
            // arrange: seed één product zonder extra opties
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            await using var ctxSeed = new AppDbContext(options);
            var p = new Product { ProductID = 7, Name = "X", InStock = 1, MinStock = 0, BasePrice = 10m, Description = "D" };
            ctxSeed.Products.Add(p);
            await ctxSeed.SaveChangesAsync();

            var svc = new ProductService(ctxSeed);
            var ctrl = new ProductsController(svc);

            var req = new PriceCalculationRequestDTO
            {
                Quantity = 2,
                SelectedOptionIds = new List<int>(),
                OptionValues = new Dictionary<int, string>(),
                CustomText = null
            };

            // act
            var result = await ctrl.CalculatePrice(7, req);

            // assert
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var price = Assert.IsType<PriceDTO>(ok.Value);
            Assert.Equal(10m, price.UnitPrice);
            Assert.Equal(20m, price.TotalPrice);
        }
    }
}
