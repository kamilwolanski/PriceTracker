using PriceTracker.Features.TrackedProducts.DTOs;

namespace PriceTracker.Features.TrackedProductCreation
{
    public interface ITrackedProductCreationService
    {
        Task<CreateTrackedProductResult> CreateAsync(CreateTrackedProductDto dto, Guid userId);
    }
}
