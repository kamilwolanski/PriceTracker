using Microsoft.EntityFrameworkCore;
using PriceTracker.Data;
using PriceTracker.Features.PriceHistory.DTOs;
using PriceTracker.Models;

namespace PriceTracker.Features.PriceHistory
{
    public class PriceHistoryService
    {
        private readonly AppDbContext _context;
        public PriceHistoryService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PriceHistoryDto?> GetByIdAsync(Guid id, Guid userId)
        {
            return await _context.PriceHistories
                .Where(ph => ph.Id == id && ph.TrackedProduct.UserId == userId)
                .Select(ph => new PriceHistoryDto
                {
                    Id = ph.Id,
                    Price = ph.Price,
                    CheckedAt = ph.CheckedAt,
                    TrackedProductId = ph.TrackedProductId
                })
                .FirstOrDefaultAsync();
        }

        public async Task<PriceHistoryDto?> AddAsync(AddPriceHistoryDto dto, Guid userId)
        {
            var trackedProductExists = await _context.TrackedProducts
                .AnyAsync(tp => tp.Id == dto.TrackedProductId && tp.UserId == userId);

            if (!trackedProductExists)
                return null;

            var priceHistory = new Models.PriceHistory
            {
                Id = Guid.NewGuid(),
                Price = dto.Price,
                CheckedAt = DateTime.UtcNow,
                TrackedProductId = dto.TrackedProductId
            };

            _context.Add(priceHistory);
            await _context.SaveChangesAsync();

            return new PriceHistoryDto
            {
                Id = priceHistory.Id,
                Price = priceHistory.Price,
                CheckedAt = priceHistory.CheckedAt,
                TrackedProductId = priceHistory.TrackedProductId
            };
        }

        public async Task<PriceHistoryDto?> UpdateAsync(Guid id, UpdatePriceHistoryDto dto, Guid userId)
        {
            var priceHistory = await _context.PriceHistories
                .FirstOrDefaultAsync(ph => ph.Id == id && ph.TrackedProduct.UserId == userId);

            if (priceHistory == null)
                return null;

            priceHistory.Price = dto.Price;
            priceHistory.CheckedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return new PriceHistoryDto
            {
                Id = priceHistory.Id,
                Price = priceHistory.Price,
                CheckedAt = priceHistory.CheckedAt,
                TrackedProductId = priceHistory.TrackedProductId
            };
        }

        public async Task<bool> DeleteAsync(Guid id, Guid userId)
        {
            var priceHistory = await _context.PriceHistories
                .FirstOrDefaultAsync(ph => ph.Id == id && ph.TrackedProduct.UserId == userId);

            if (priceHistory == null)
                return false;

            _context.PriceHistories.Remove(priceHistory);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
