using Microsoft.EntityFrameworkCore;
using Webshop.Backend.Data;
using Webshop.Shared.DTOs;
using Webshop.Shared.Models;

namespace Webshop.Backend.Services
{
    public class OrderService
    {
        private readonly AppDbContext _context;

        public OrderService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<OrderDTO.Index>> GetOrdersByUserAsync(int userId)
        {
            var orders = await _context.Orders
                .Where(o => o.UserID == userId)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Options)
                .ToListAsync();

            return orders.Select(order => new OrderDTO.Index
            {
                OrderID = order.OrderID,
                OrderDate = order.OrderDate,
                TotalPrice = order.TotalPrice,
                Items = order.Items.Select(item => new OrderItemDTO.Index
                {
                    ProductID = item.ProductID,
                    Quantity = item.Quantity,
                    Price = item.Price,
                    Options = item.Options.Select(opt => new OrderItemOptionDTO.Index
                    {
                        OptionID = opt.OptionID,
                        OptionType = opt.OptionKey,
                        OptionValue = opt.OptionKey == "CustomText"
                                      ? opt.CustomTextValue ?? ""
                                      : opt.OptionValue
                    }).ToList()
                }).ToList()
            }).ToList();
        }


        public async Task<OrderDTO.Created> CreateOrderAsync(OrderDTO.Create dto)
        {
            var order = new Order
            {
                UserID = dto.UserID,
                OrderDate = dto.OrderDate
            };

            foreach (var it in dto.Items)
            {
                var item = new OrderItem
                {
                    ProductID = it.ProductID,
                    Quantity = it.Quantity,
                    Price = it.UnitPrice
                };

                foreach (var opt in it.Options)
                {
                    item.Options.Add(new OrderItemOption
                    {
                        OptionID = opt.OptionID,
                        OptionKey = opt.Key,
                        OptionValue = opt.Value,
                        CustomTextValue = opt.Key == "CustomText" ? opt.Value : null
                    });
                }

                order.Items.Add(item);

                // voorraad bijwerken…
                var product = await _context.Products.FindAsync(it.ProductID);
                if (product != null) product.InStock -= it.Quantity;
            }

            order.TotalPrice = order.Items.Sum(i => i.Quantity * i.Price);
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            return new OrderDTO.Created { OrderID = order.OrderID };
        }
    }
}
