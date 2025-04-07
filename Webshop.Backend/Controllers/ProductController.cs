using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Webshop.Backend.Data;
using Webshop.Shared.DTOs;
using Webshop.Shared.Models;

namespace Webshop.Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProductsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductDTO.Index>>> GetProducts(int page = 1, int pageSize = 10)
        {
            var products = await _context.Products
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new ProductDTO.Index
                {
                    ProductID = p.ProductID,
                    Name = p.Name,
                    InStock = p.InStock
                }).ToListAsync();

            return Ok(products);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProductDTO.Index>> GetProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();
            return Ok(product);
        }
    }

}
