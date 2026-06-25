using Microsoft.EntityFrameworkCore;
using PriceTracker.Data;
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

        public async Task<List<TrackedProductDto>> GetAllTrackedProductsAsync()
        {
            return await _context.TrackedProducts.Select(tp => new TrackedProductDto
            {
                Id = tp.Id,
                Name = tp.Name,
                Url = tp.Url,
            }).ToListAsync();
        }

        public async Task<TrackedProductDto?> GetByIdAsync(Guid id)
        {
            return await _context.TrackedProducts
                .Where(tp => tp.Id == id)
                .Select(tp => new TrackedProductDto()
                {
                    Id = tp.Id,
                    Name = tp.Name,
                    Url = tp.Url,
                })
                .FirstOrDefaultAsync();
        }

        public async Task<TrackedProductDto> AddAsync(CreateTrackedProductDto dto)
        {
            var trackedProduct = new TrackedProduct
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Url = dto.Url
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

        public async Task<TrackedProductDto?> UpdateAsync(Guid id, UpdateTrackedProductDto dto)
        {
            var trackedProduct = await _context.TrackedProducts.FindAsync(id);
            if (trackedProduct == null)
                return null;

            trackedProduct.Name = dto.Name;
            trackedProduct.Url = dto.Url;
            await _context.SaveChangesAsync();

            return new TrackedProductDto
            {
                Id = trackedProduct.Id,
                Name = trackedProduct.Name,
                Url = trackedProduct.Url
            };
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var trackedProduct = await _context.TrackedProducts.FindAsync(id);
            if (trackedProduct == null)
                return false;

            _context.TrackedProducts.Remove(trackedProduct);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
