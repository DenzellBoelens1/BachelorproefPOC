using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using Webshop.Shared.Models;

namespace Webshop.Backend.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<Product> Products { get; set; } = null!;
        public DbSet<ProductOption> ProductOptions { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Order> Orders { get; set; } = null!;
        public DbSet<OrderItem> OrderItems { get; set; } = null!;
        public DbSet<OrderItemOption> OrderItemOptions { get; set; } = null!;

    }

}
