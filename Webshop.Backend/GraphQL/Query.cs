using HotChocolate;
using HotChocolate.Types;
using Webshop.Backend.Data;
using Webshop.Backend.Services;
using Webshop.Shared.DTOs;

namespace Webshop.Backend.GraphQL
{
    public class Query
    {
        [UsePaging(IncludeTotalCount = true)]
        public IQueryable<ProductDTO.Index> GetProducts(string? search, [Service] AppDbContext context)
        {
            var query = context.Products.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var lowerSearch = search.ToLower();
                query = query.Where(p => p.Name.ToLower().Contains(lowerSearch));
            }

            return query.Select(p => new ProductDTO.Index
            {
                ProductID = p.ProductID,
                Name = p.Name,
                InStock = p.InStock
            });
        }

        public Task<ProductDTO.Index?> GetProductById(int id, [Service] ProductService service)
            => service.GetProductIndexAsync(id);

        public Task<ProductDTO.Details?> GetProductDetails(int id, [Service] ProductService service)
            => service.GetProductDetailsAsync(id);

        [GraphQLName("getOrdersByUser")]
        public Task<List<OrderDTO.Index>> GetOrdersByUser(int userId, [Service] OrderService service)
        => service.GetOrdersByUserAsync(userId);
    }

    public class Mutation
    {
        public Task<ProductDTO.Index?> UpdateProductStock(int productID, int inStock, [Service] ProductService service)
            => service.UpdateStockAsync(productID, inStock);

        [GraphQLName("placeOrder")]
        public async Task<OrderDTO.Created> PlaceOrder(OrderDTO.Create orderDto, [Service] OrderService service)
        {
            return await service.CreateOrderAsync(orderDto);
        }

        [GraphQLName("calculatePrice")]
        public async Task<PriceDTO> CalculatePrice(
            int productId,
            int quantity,
            List<int> selectedOptionIds,
            Dictionary<int, string> optionValues,
            string? customText,
            [Service] ProductService service)
        {
            var (unit, total) = await service.CalculatePriceAsync(
                productId, quantity, selectedOptionIds, optionValues, customText);
            return new PriceDTO { UnitPrice = unit, TotalPrice = total };
        }
    }
}
