using Microsoft.EntityFrameworkCore;
using PriceTracker.Data;
using PriceTracker.Models;

namespace PriceTracker.Services
{
    public class TrackedProductService
    {
        private readonly AppDbContext _context;
        public TrackedProductService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<TrackedProduct>> GetAllTrackedProductsAsync()
        {
            return await _context.TrackedProducts.ToListAsync();
        }
    }

}
