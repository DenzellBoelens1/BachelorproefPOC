using Microsoft.EntityFrameworkCore;
using Webshop.Backend.Data;
using Webshop.Shared.DTOs;

namespace Webshop.Backend.Services
{
    public class ProductService
    {
        private readonly AppDbContext _context;

        public ProductService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<ProductDTO.Index>> GetProductsAsync(int page, int pageSize, string? search = null)
        {
            var query = _context.Products.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(p => p.Name.Contains(search));
            }

            return await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new ProductDTO.Index
                {
                    ProductID = p.ProductID,
                    Name = p.Name,
                    InStock = p.InStock
                }).ToListAsync();
        }

        public async Task<ProductDTO.Index?> GetProductIndexAsync(int id)
        {
            return await _context.Products
                .Where(p => p.ProductID == id)
                .Select(p => new ProductDTO.Index
                {
                    ProductID = p.ProductID,
                    Name = p.Name,
                    InStock = p.InStock
                })
                .FirstOrDefaultAsync();
        }

        public async Task<ProductDTO.Details?> GetProductDetailsAsync(int id)
        {
            var product = await _context.Products
                .Where(p => p.ProductID == id)
                .Select(p => new ProductDTO.Details
                {
                    ProductID = p.ProductID,
                    Name = p.Name,
                    Description = p.Description,
                    BasePrice = p.BasePrice,
                    InStock = p.InStock,
                    Options = _context.ProductOptions
                        .Where(o => o.ProductID == p.ProductID)
                        .GroupBy(o => o.OptionType)
                        .Select(g => new ProductDTO.OptionGroup
                        {
                            OptionType = g.Key,
                            Values = g.Select(o => o.OptionValue).ToList()
                        }).ToList()
                })
                .FirstOrDefaultAsync();

            return product;
        }

        public async Task<ProductDTO.Index?> UpdateStockAsync(int productId, int inStock)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null) return null;

            product.InStock = inStock;
            await _context.SaveChangesAsync();

            return new ProductDTO.Index
            {
                ProductID = product.ProductID,
                Name = product.Name,
                InStock = product.InStock
            };
        }
    }
}

