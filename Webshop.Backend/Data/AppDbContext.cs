using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using Webshop.Shared.Models;

namespace Webshop.Backend.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<Product> Products { get; set; } = null!;
    }

}
