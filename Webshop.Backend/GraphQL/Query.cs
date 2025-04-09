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
    }

    public class Mutation
    {
        public Task<ProductDTO.Index?> UpdateProductStock(int productID, int inStock, [Service] ProductService service)
            => service.UpdateStockAsync(productID, inStock);
    }
}
