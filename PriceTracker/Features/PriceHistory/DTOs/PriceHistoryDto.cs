using PriceTracker.Features.PriceHistory.ValueObjects;

namespace PriceTracker.Features.PriceHistory.DTOs
{
    public class PriceHistoryDto
    {
        public Guid Id { get; set; }
        public Money Price { get; set; }
        public DateTime CheckedAt { get; set; }
        public Guid TrackedProductId { get; set; }
    }
}
