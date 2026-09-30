using Microsoft.EntityFrameworkCore;
using PriceTracker.Data;
using PriceTracker.Features.PriceHistory.DTOs;
using PriceTracker.Features.PriceHistory.ValueObjects;
using PriceTracker.Features.TrackedProducts.DTOs;
using PriceTracker.Models;
using Microsoft.Extensions.Options;
using PriceTracker.Features.PriceMonitoring;
using PriceTracker.Common.DTOs;

namespace PriceTracker.Features.TrackedProducts
{
    public class TrackedProductService : ITrackedProductService
    {
        private readonly PriceMonitoringOptions _options;
        private readonly AppDbContext _context;
        public TrackedProductService(AppDbContext context, IOptions<PriceMonitoringOptions> options)
        {
            _context = context;
            _options = options.Value;
        }

        private Task<bool> ExistAsync(Guid id, Guid userId)
        {
            return _context.TrackedProducts.AnyAsync(tp => tp.Id == id && tp.UserId == userId);
        }

        public async Task<PaginatedResult<TrackedProductDto>> GetAllTrackedProductsAsync(Guid userId, int page, int limit)
        {
            var totalCount = await _context.TrackedProducts
                .CountAsync(tp => tp.UserId == userId);

            var totalPages = (int)Math.Ceiling((double)totalCount / limit);

            var trackedProducts = await _context.TrackedProducts
                .Where(tp => tp.UserId == userId)
                .OrderByDescending(tp => tp.Id)     
                .Skip((page - 1) * limit)
                .Take(limit)
                .Select(tp => new TrackedProductDto
                {
                    Id = tp.Id,
                    Name = tp.Name,
                    Url = tp.Url,
                    CurrentPrice = tp.PriceHistory
                        .OrderByDescending(ph => ph.CheckedAt)
                        .Select(ph => (Money?)ph.Price)
                        .FirstOrDefault(),
                    LastCheckedAt = tp.LastCheckedAt
                })
                .ToListAsync();

            return new PaginatedResult<TrackedProductDto>
            {
                Items = trackedProducts,
                Limit = limit,
                TotalCount = totalCount,
                Page = page,
                TotalPages = totalPages
            };
        }

        public async Task<List<TrackedProduct>> GetProductsForPriceCheckAsync(
            int skip,
            int take)
        {
            var now = DateTime.UtcNow;

            return await _context.TrackedProducts
                .Where(tp => tp.NextCheckAt == null || tp.NextCheckAt <= now)
                .OrderBy(tp => tp.Id)
                .Skip(skip)
                .Take(take)
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
                        .Select(ph => (Money?)ph.Price)
                        .FirstOrDefault(),
                    LastCheckedAt = tp.LastCheckedAt
                })
                .FirstOrDefaultAsync();
        }

        public async Task<List<PriceHistoryDto>?> GetPriceHistoryAsync(Guid trackedProductId, Guid userId)
        {
            var exists = await ExistAsync(trackedProductId, userId);

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

        public async Task<PriceHistoryStatisticsResult> GetPriceHistoryStatisticsAsync(
            Guid trackedProductId,
            Guid userId)
        {
            var exists = await ExistAsync(trackedProductId, userId);

            if (!exists)
            {
                return new PriceHistoryStatisticsResult
                {
                    ProductExists = false
                };
            }

            var statistics = await _context.PriceHistories
                .Where(ph => ph.TrackedProductId == trackedProductId)
                .GroupBy(_ => 1)
                .Select(g => new PriceHistoryStatistics
                {
                    Min = g.Min(ph => ph.Price.Amount),
                    Max = g.Max(ph => ph.Price.Amount),
                    Average = Math.Round(g.Average(ph => ph.Price.Amount), 2)
                })
                .FirstOrDefaultAsync();

            return new PriceHistoryStatisticsResult
            {
                ProductExists = true,
                Statistics = statistics
            };
        }

        public async Task<TrackedProductDto> AddAsync(CreateTrackedProductDto dto, Guid userId)
        {
            var trackedProduct = new TrackedProduct
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Url = dto.Url,
                UserId = userId,
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

        public async Task<DateTime> UpdateAfterPriceCheckAsync(Guid id)
        {
            var trackedProduct = await _context.TrackedProducts
                .SingleAsync(tp => tp.Id == id);

            var checkedAt = DateTime.UtcNow;

            trackedProduct.LastCheckedAt = checkedAt;
            trackedProduct.NextCheckAt =
                    checkedAt.AddSeconds(_options.CheckIntervalSeconds);

            await _context.SaveChangesAsync();

            return checkedAt;
        }
    }
}
