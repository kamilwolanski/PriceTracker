using Microsoft.AspNetCore.Mvc;
using PriceTracker.Features.PriceHistory.DTOs;
using PriceTracker.Features.TrackedProducts.DTOs;
using PriceTracker.Models;

namespace PriceTracker.Features.TrackedProducts
{
    public interface ITrackedProductService
    {
        Task<List<TrackedProductDto>> GetAllTrackedProductsAsync(Guid userId);
        Task<List<TrackedProduct>> GetProductsForPriceCheckAsync(int skip, int take);
        Task<TrackedProductDto?> GetByIdAsync(Guid id, Guid userId);
        Task<List<PriceHistoryDto>?> GetPriceHistoryAsync(Guid trackedProductId, Guid userId);
        Task<TrackedProductDto> AddAsync(CreateTrackedProductDto dto, Guid userId);
        Task<TrackedProductDto?> UpdateAsync(Guid id, UpdateTrackedProductDto dto, Guid userId);
        Task<bool> DeleteAsync(Guid id, Guid userId);
        Task<DateTime> UpdateAfterPriceCheckAsync(Guid id);
    }
}
