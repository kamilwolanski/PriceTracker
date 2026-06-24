using Microsoft.EntityFrameworkCore;
using PriceTracker.Models;

namespace PriceTracker.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<TrackedProduct> TrackedProducts { get; set; }
        public DbSet<PriceHistory> PriceHistories
        {
            get; set;
        }
}
}
