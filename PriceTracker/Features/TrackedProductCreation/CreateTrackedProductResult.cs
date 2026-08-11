using PriceTracker.Features.PriceChecking;
using PriceTracker.Features.TrackedProducts.DTOs;

namespace PriceTracker.Features.TrackedProductCreation
{
    public enum CreateTrackedProductStatus
    {
        Success,
        ScrapeFailed
    }
    public class CreateTrackedProductResult
    {
        public TrackedProductDto? Product { get; set; }
        public CreateTrackedProductStatus  Status { get; set; }
    }
}
