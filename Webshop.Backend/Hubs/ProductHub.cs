using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Webshop.Backend.Data;
using Webshop.Shared.DTOs;

namespace Webshop.Backend.Hubs
{
    public class ProductHub : Hub
    {
        private readonly AppDbContext _context;

        public ProductHub(AppDbContext context)
        {
            _context = context;
        }

        public async Task GetProducts(int page = 1, int pageSize = 10)
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

            await Clients.Caller.SendAsync("ReceiveProducts", products);
        }
    }

}
