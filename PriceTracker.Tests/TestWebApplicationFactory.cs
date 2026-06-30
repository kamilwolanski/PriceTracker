using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PriceTracker.Data;

namespace PriceTracker.Tests
{
    public class TestWebApplicationFactory : WebApplicationFactory<Program>
    {
        // Jedna nazwa bazy na całą instancję fabryki —
        // wszystkie requesty w ramach jednej klasy testowej dzielą tę samą bazę
        private readonly string _dbName = Guid.NewGuid().ToString();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Usuń istniejące rejestracje DbContext i opcji
                var toRemove = services
                    .Where(d =>
                        d.ServiceType == typeof(AppDbContext) ||
                        d.ServiceType == typeof(DbContextOptions<AppDbContext>))
                    .ToList();

                foreach (var descriptor in toRemove)
                    services.Remove(descriptor);

                // Zarejestruj AppDbContext z bazą in-memory
                services.AddScoped(_ =>
                {
                    var options = new DbContextOptionsBuilder<AppDbContext>()
                        .UseInMemoryDatabase(_dbName)
                        .Options;
                    return new AppDbContext(options);
                });
            });
        }
    }
}
