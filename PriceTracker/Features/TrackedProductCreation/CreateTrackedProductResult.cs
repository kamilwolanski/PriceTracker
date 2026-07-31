using PriceTracker.Features.TrackedProducts.DTOs;

namespace PriceTracker.Features.TrackedProductCreation
{
    public class CreateTrackedProductResult
    {
        public bool Success { get; set; }
        public TrackedProductDto? Product { get; set; }
        public bool InitialPriceChecked { get; set; }
        public string? Error { get; set; }
    }
}
