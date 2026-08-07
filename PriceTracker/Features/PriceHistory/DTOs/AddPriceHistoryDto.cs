using PriceTracker.Features.PriceHistory.ValueObjects;

namespace PriceTracker.Features.PriceHistory.DTOs
{
    public class AddPriceHistoryDto
    {
        public Guid TrackedProductId { get; set; }

        public Money Price { get; set; }
    }
}

