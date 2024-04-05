using Microsoft.EntityFrameworkCore;
using Order.Api.Models.Entities;

namespace Order.Api.Models.Contexts
{
    public class OrderDbContext : DbContext
    {
        public OrderDbContext(DbContextOptions options) : base(options)
        {
        }

        override protected void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<OrderOutbox>().HasKey(o => o.IdempotentToken);
        }
        public DbSet<Entities.Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<OrderOutbox> OrderOutboxes { get; set; }
    }
}
