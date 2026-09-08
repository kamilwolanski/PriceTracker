using Microsoft.EntityFrameworkCore;
using PriceTracker.Data;
using PriceTracker.Features.PriceHistory.DTOs;
using PriceTracker.Features.PriceHistory.ValueObjects;
using PriceTracker.Models;

namespace PriceTracker.Features.PriceHistory
{
    public class PriceHistoryService : IPriceHistoryService
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

        public async Task<PriceHistoryDto?> GetTheLastPriceAsync(Guid productId)
        {
            return await _context.PriceHistories
                .Where(x => x.TrackedProductId == productId)
                .OrderByDescending(x => x.CheckedAt)
                .Select(ph => new PriceHistoryDto
                {
                    Id = ph.Id,
                    Price = ph.Price,
                    CheckedAt = ph.CheckedAt,
                    TrackedProductId = ph.TrackedProductId
                })
                .FirstOrDefaultAsync();
        }

        public async Task<PriceHistoryDto?> AddAsync(
            AddPriceHistoryDto dto,
            Guid userId)
        {
            if (!await ProductBelongsToUserAsync(dto.TrackedProductId, userId))
                return null;

            var history = AddEntry(
                dto.TrackedProductId,
                dto.Price,
                DateTime.UtcNow);

            await _context.SaveChangesAsync();

            return history;
        }


        private async Task<bool> ProductBelongsToUserAsync(Guid trackedProductId, Guid userId)
        {
            return await _context.TrackedProducts
                .AnyAsync(tp => tp.Id == trackedProductId && tp.UserId == userId);
        }

        private PriceHistoryDto AddEntry(
            Guid trackedProductId,
            Money price,
            DateTime checkedAt)
        {
            var priceHistory = new Models.PriceHistory
            {
                Id = Guid.NewGuid(),
                Price = price,
                CheckedAt = checkedAt,
                TrackedProductId = trackedProductId
            };

            _context.PriceHistories.Add(priceHistory);

            return new PriceHistoryDto
            {
                Id = priceHistory.Id,
                Price = priceHistory.Price,
                CheckedAt = priceHistory.CheckedAt,
                TrackedProductId = priceHistory.TrackedProductId
            };
        }

        public async Task AddPriceCheckAsync(
            Guid trackedProductId,
            Money price,
            DateTime checkedAt
            )
        {

            var lastPrice = await _context.PriceHistories
                .Where(ph => ph.TrackedProductId == trackedProductId)
                .OrderByDescending(ph => ph.CheckedAt)
                .Select(ph => ph.Price)
                .FirstOrDefaultAsync();

            if (lastPrice != price)
            {
                AddEntry(trackedProductId, price, checkedAt);
            }

            await _context.SaveChangesAsync();
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
