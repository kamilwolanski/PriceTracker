using PriceTracker.Data;
using PriceTracker.DTOs.PriceHistory;
using PriceTracker.Models;
using Microsoft.EntityFrameworkCore; // <- dodaj to

namespace PriceTracker.Services
{
    public class PriceHistoryService
    {
        private readonly AppDbContext _context;
        public PriceHistoryService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PriceHistoryDto?> GetByIdAsync(Guid id)
        {
            return await _context.PriceHistories
                .Where(ph => ph.Id == id)
                .Select(ph => new PriceHistoryDto
                {
                    Id = ph.Id,
                    Price = ph.Price,
                    CheckedAt = ph.CheckedAt,
                    TrackedProductId = ph.TrackedProductId
                })
                .FirstOrDefaultAsync();
        }

        public async Task<PriceHistoryDto> AddAsync(AddPriceHistoryDto dto)
        {
            var priceHistory = new PriceHistory()
            {
                Id = Guid.NewGuid(),
                Price = dto.Price,
                CheckedAt = DateTime.UtcNow,
                TrackedProductId = dto.TrackedProductId
            };

            _context.Add(priceHistory);
            await _context.SaveChangesAsync();

            var priceHistoryDto = new PriceHistoryDto()
            {
                Id = priceHistory.Id,
                Price = priceHistory.Price,
                CheckedAt = priceHistory.CheckedAt,
                TrackedProductId = priceHistory.TrackedProductId
            };

            return priceHistoryDto;
        }
    }
}
