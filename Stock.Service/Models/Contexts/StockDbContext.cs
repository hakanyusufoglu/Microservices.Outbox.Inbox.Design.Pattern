using Microsoft.EntityFrameworkCore;

namespace Stock.Service.Models.Contexts
{
    public class StockDbContext :DbContext
    {
        public StockDbContext(DbContextOptions<StockDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Entities.OrderInbox>().HasKey(o => o.IdempotentToken);
        }
        public DbSet<Entities.OrderInbox> OrderInboxes { get; set; }
    }
}
