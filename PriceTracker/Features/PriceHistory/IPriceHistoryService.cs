using PriceTracker.Features.PriceHistory.DTOs;
using PriceTracker.Features.PriceHistory.ValueObjects;

namespace PriceTracker.Features.PriceHistory
{
    public interface IPriceHistoryService
    {
        Task<PriceHistoryDto?> GetByIdAsync(Guid id, Guid userId);
        Task<PriceHistoryDto?> AddAsync(
            AddPriceHistoryDto dto,
            Guid userId);
        Task AddPriceCheckAsync(
            Guid trackedProductId,
            Money price,
            DateTime checkedAt
            );
        Task<PriceHistoryDto?> UpdateAsync(Guid id, UpdatePriceHistoryDto dto, Guid userId);
        Task<bool> DeleteAsync(Guid id, Guid userId);
    }
}
