using HotChocolate;
using HotChocolate.Types;
using System.Linq;
using Webshop.Backend.Data;
using Webshop.Shared.DTOs;
using Webshop.Shared.Models;

namespace Webshop.Backend.GraphQL
{
    public class Query
    {
        [UsePaging(IncludeTotalCount = true)]
        [UseFiltering]
        [UseSorting]
        public IQueryable<ProductDTO.Index> GetProducts([Service] AppDbContext context)
            => context.Products.Select(p => new ProductDTO.Index
                {
                    ProductID = p.ProductID,
                    Name = p.Name,
                    InStock = p.InStock
                });

        public async Task<ProductDTO.Index?> GetProduct(int id, [Service] AppDbContext context)
        {
            var product = await context.Products.FindAsync(id);
            return product == null ? null : new ProductDTO.Index
            {
                ProductID = product.ProductID,
                Name = product.Name,
                InStock = product.InStock
            };
        }
    }

}
