using Microsoft.EntityFrameworkCore;
using Webshop.Backend.Data;
using Webshop.Shared.DTOs;

namespace Webshop.Backend.Services
{
    public class OrderService
    {
        private readonly AppDbContext _context;

        public OrderService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<OrderDTO>> GetOrdersByUserAsync(int userId)
        {
            var orders = await _context.Orders
                .Where(o => o.UserID == userId)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Options)
                .ToListAsync();

            var allOptionIds = orders
                .SelectMany(o => o.Items)
                .SelectMany(i => i.Options)
                .Select(o => o.OptionID)
                .Distinct()
                .ToList();

            var optionDetails = await _context.ProductOptions
                .Where(po => allOptionIds.Contains(po.OptionID))
                .ToDictionaryAsync(po => po.OptionID);

            var result = orders.Select(order => new OrderDTO
            {
                OrderID = order.OrderID,
                OrderDate = order.OrderDate,
                TotalPrice = order.TotalPrice,
                Items = order.Items.Select(item => new OrderItemDTO
                {
                    ProductID = item.ProductID,
                    Quantity = item.Quantity,
                    Price = item.Price,
                    Options = item.Options.Select(opt => new OrderItemOptionDTO
                    {
                        OptionID = opt.OptionID,
                        OptionType = optionDetails.TryGetValue(opt.OptionID, out var optData) ? optData.OptionType : "Onbekend",
                        OptionValue = optionDetails.TryGetValue(opt.OptionID, out var optData2) ? optData2.OptionValue : ""
                    }).ToList()
                }).ToList()
            }).ToList();

            return result;
        }
    }
}
