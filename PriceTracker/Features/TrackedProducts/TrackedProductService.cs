using Microsoft.EntityFrameworkCore;
using PriceTracker.Data;
using PriceTracker.Features.PriceHistory.DTOs;
using PriceTracker.Features.TrackedProducts.DTOs;
using PriceTracker.Models;

namespace PriceTracker.Features.TrackedProducts
{
    public class TrackedProductService
    {
        private readonly AppDbContext _context;
        public TrackedProductService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<TrackedProductDto>> GetAllTrackedProductsAsync(Guid userId)
        {
            return await _context.TrackedProducts
                .Where(tp => tp.UserId == userId)
                .Select(tp => new TrackedProductDto
                {
                    Id = tp.Id,
                    Name = tp.Name,
                    Url = tp.Url,
                    CurrentPrice = tp.PriceHistory
                        .OrderByDescending(ph => ph.CheckedAt)
                        .Select(ph => (decimal?)ph.Price)
                        .FirstOrDefault(),
                    LastCheckedAt = tp.PriceHistory
                        .OrderByDescending(ph => ph.CheckedAt)
                        .Select(ph => (DateTime?)ph.CheckedAt)
                        .FirstOrDefault(),
                })
                .ToListAsync();
        }

        public async Task<TrackedProductDto?> GetByIdAsync(Guid id, Guid userId)
        {
            return await _context.TrackedProducts
                .Where(tp => tp.Id == id && tp.UserId == userId)
                .Select(tp => new TrackedProductDto
                {
                    Id = tp.Id,
                    Name = tp.Name,
                    Url = tp.Url,
                    CurrentPrice = tp.PriceHistory
                        .OrderByDescending(ph => ph.CheckedAt)
                        .Select(ph => (decimal?)ph.Price)
                        .FirstOrDefault(),
                    LastCheckedAt = tp.PriceHistory
                        .OrderByDescending(ph => ph.CheckedAt)
                        .Select(ph => (DateTime?)ph.CheckedAt)
                        .FirstOrDefault(),
                })
                .FirstOrDefaultAsync();
        }

        public async Task<List<PriceHistoryDto>?> GetPriceHistoryAsync(Guid trackedProductId, Guid userId)
        {
            var exists = await _context.TrackedProducts
                .AnyAsync(tp => tp.Id == trackedProductId && tp.UserId == userId);

            if (!exists)
                return null;

            return await _context.PriceHistories
                .Where(ph => ph.TrackedProductId == trackedProductId)
                .OrderByDescending(ph => ph.CheckedAt)
                .Select(ph => new PriceHistoryDto
                {
                    Id = ph.Id,
                    Price = ph.Price,
                    CheckedAt = ph.CheckedAt,
                    TrackedProductId = trackedProductId,
                })
                .ToListAsync();
        }

        public async Task<TrackedProductDto> AddAsync(CreateTrackedProductDto dto, Guid userId)
        {
            var trackedProduct = new TrackedProduct
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Url = dto.Url,
                UserId = userId
            };

            _context.TrackedProducts.Add(trackedProduct);
            await _context.SaveChangesAsync();

            return new TrackedProductDto
            {
                Id = trackedProduct.Id,
                Name = trackedProduct.Name,
                Url = trackedProduct.Url
            };
        }

        public async Task<TrackedProductDto?> UpdateAsync(Guid id, UpdateTrackedProductDto dto, Guid userId)
        {
            var trackedProduct = await _context.TrackedProducts
                .FirstOrDefaultAsync(tp => tp.Id == id && tp.UserId == userId);

            if (trackedProduct == null)
                return null;

            trackedProduct.Name = dto.Name;
            trackedProduct.Url = dto.Url;
            await _context.SaveChangesAsync();

            return await GetByIdAsync(id, userId);
        }

        public async Task<bool> DeleteAsync(Guid id, Guid userId)
        {
            var trackedProduct = await _context.TrackedProducts
                .FirstOrDefaultAsync(tp => tp.Id == id && tp.UserId == userId);

            if (trackedProduct == null)
                return false;

            _context.TrackedProducts.Remove(trackedProduct);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
