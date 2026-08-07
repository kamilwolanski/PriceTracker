using PriceTracker.Features.PriceHistory.ValueObjects;

namespace PriceTracker.Features.PriceHistory.DTOs
{
    public class UpdatePriceHistoryDto
    {
        public Money Price { get; set; }
    }
}

