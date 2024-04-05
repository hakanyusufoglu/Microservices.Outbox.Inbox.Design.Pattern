using Microsoft.EntityFrameworkCore;

namespace Stock.Service.Models.Contexts
{
    public class StockDbContext :DbContext
    {
        public StockDbContext(DbContextOptions<StockDbContext> options) : base(options)
        {
        }

        public DbSet<Entities.OrderInbox> OrderInboxes { get; set; }
    }
}
