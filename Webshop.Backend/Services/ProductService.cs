using Microsoft.EntityFrameworkCore;
using Webshop.Backend.Data;
using Webshop.Shared.DTOs;
using static Webshop.Shared.DTOs.ProductDTO;
using Webshop.Shared.Models;
using System.Globalization;

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
            return await _context.Products
                .Where(p => p.ProductID == id)
                .Select(p => new ProductDTO.Details
                {
                    ProductID = p.ProductID,
                    Name = p.Name,
                    InStock = p.InStock,
                    BasePrice = p.BasePrice,
                    Options = p.Options.Select(po => new ProductDTO.OptionDetail
                    {
                        OptionID = po.OptionID,
                        OptionType = po.OptionType,
                        OptionValue = po.OptionValue
                    }).ToList()
                })
                .FirstOrDefaultAsync();
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

        public async Task<(decimal UnitPrice, decimal TotalPrice)> CalculatePriceAsync(
            int productId,
            int quantity,
            IReadOnlyList<int> optionIds,
            IReadOnlyDictionary<int, string> optionValues,
            string? customText)
        {
            var details = await GetProductDetailsAsync(productId);
            if (details is null)
                throw new ArgumentException("Product niet gevonden", nameof(productId));

            // 1) Basisprijs
            decimal unitPrice = details.BasePrice;

            // 2) Extra kosten voor opties
            foreach (var optId in optionIds)
            {
                var opt = details.Options.FirstOrDefault(o => o.OptionID == optId);
                if (opt == null) continue;
                if (decimal.TryParse(opt.OptionValue,
                                     NumberStyles.Any,
                                     CultureInfo.InvariantCulture,
                                     out var extra))
                {
                    unitPrice += extra;
                }
            }

            // 3) Extra kosten voor custom tekst
            if (!string.IsNullOrWhiteSpace(customText))
            {
                var textOpt = details.Options
                    .FirstOrDefault(o => o.OptionType == "CustomText" &&
                                         o.OptionValue.StartsWith("PricePerCharacter=", StringComparison.OrdinalIgnoreCase));
                if (textOpt != null)
                {
                    var parts = textOpt.OptionValue.Split('=');
                    if (parts.Length == 2 &&
                        decimal.TryParse(parts[1], NumberStyles.Any, CultureInfo.InvariantCulture, out var perChar))
                    {
                        unitPrice += perChar * customText.Length;
                    }
                }
            }

            // 4) Totale prijs
            decimal totalPrice = unitPrice * quantity;
            return (unitPrice, totalPrice);
        }

    }
}

