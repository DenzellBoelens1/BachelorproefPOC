using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Webshop.Backend.Services;
using Webshop.Shared.DTOs;

namespace Webshop.Backend.Hubs
{
    public class ProductHub : Hub
    {
        private readonly ProductService _productService;

        public ProductHub(ProductService productService)
        {
            _productService = productService;
        }

        public async Task GetProducts(int page, int pageSize, string? search)
        {
            var products = await _productService.GetProductsAsync(page, pageSize, search);
            await Clients.Caller.SendAsync("ReceiveProducts", products);
        }

        public async Task GetProductById(int id)
        {
            var product = await _productService.GetProductIndexAsync(id);
            if (product == null)
                await Clients.Caller.SendAsync("ProductNotFound", id);
            else
                await Clients.Caller.SendAsync("ReceiveProduct", product);
        }

        public async Task GetProductDetails(int id)
        {
            var product = await _productService.GetProductDetailsAsync(id);
            if (product == null)
                await Clients.Caller.SendAsync("ProductNotFound", id);
            else
                await Clients.Caller.SendAsync("ReceiveProductDetails", product);
        }

        public async Task UpdateStock(int productId, int inStock)
        {
            var updated = await _productService.UpdateStockAsync(productId, inStock);
            if (updated == null)
                await Clients.Caller.SendAsync("ProductNotFound", productId);
            else
                await Clients.Caller.SendAsync("ReceiveStockUpdated", updated);
        }

        public async Task CalculatePrice(
            int productId,
            int quantity,
            List<int> selectedOptionIds,
            Dictionary<int, string> optionValues,
            string? customText)
        {
            var (unit, total) = await _productService.CalculatePriceAsync(
                productId,
                quantity,
                selectedOptionIds,
                optionValues,
                customText);

            await Clients.Caller.SendAsync(
                "ReceivePriceCalculation",
                new PriceDTO { UnitPrice = unit, TotalPrice = total }
            );
        }
    }
}
