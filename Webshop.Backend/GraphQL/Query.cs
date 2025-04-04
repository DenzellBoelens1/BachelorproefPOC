using HotChocolate;
using HotChocolate.Types;
using Webshop.Backend.Data;
using Webshop.Shared.Models;

namespace Webshop.Backend.GraphQL
{
    public class Query
    {
        [UsePaging(IncludeTotalCount = true)]
        [UseFiltering]
        [UseSorting]
        public IQueryable<Product> GetProducts([Service] AppDbContext context) => context.Products;

        public Task<Product?> GetProduct(int id, [Service] AppDbContext context) => context.Products.FindAsync(id).AsTask();
    }

}
