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

        public DbSet<User> Users { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<TrackedProduct> TrackedProducts { get; set; }
        public DbSet<PriceHistory> PriceHistories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PriceHistory>()
                .ComplexProperty(p => p.Price, price =>
                {
                    price.Property(m => m.Amount)
                        .HasPrecision(18, 2)
                        .HasColumnName("PriceAmount");

                    price.Property(m => m.CurrencyCode)
                        .HasMaxLength(3)
                        .HasColumnName("PriceCurrencyCode");
                });
        }
    }
}
