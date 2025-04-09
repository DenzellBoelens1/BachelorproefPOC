using Microsoft.AspNetCore.Mvc;
using Webshop.Shared.DTOs;
using Webshop.Backend.Services;

namespace Webshop.Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly ProductService _productService;

        public ProductsController(ProductService productService)
        {
            _productService = productService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductDTO.Index>>> GetProducts(int page = 1, int pageSize = 10, string? search = null)
        {
            var products = await _productService.GetProductsAsync(page, pageSize, search);
            return Ok(products);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProductDTO.Index>> GetProduct(int id)
        {
            var product = await _productService.GetProductIndexAsync(id);
            if (product == null) return NotFound();
            return Ok(product);
        }

        [HttpGet("details/{id}")]
        public async Task<ActionResult<ProductDTO.Details>> GetProductDetails(int id)
        {
            var product = await _productService.GetProductDetailsAsync(id);
            if (product == null) return NotFound();
            return Ok(product);
        }

        [HttpPut("{id}/stock")]
        public async Task<ActionResult<ProductDTO.Index>> UpdateStock(int id, [FromBody] ProductDTO.UpdateStock dto)
        {
            var updated = await _productService.UpdateStockAsync(id, dto.InStock);
            if (updated == null) return NotFound();
            return Ok(updated);
        }
    }
}
