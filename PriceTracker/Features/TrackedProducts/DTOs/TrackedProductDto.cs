using PriceTracker.Features.PriceHistory.ValueObjects;

namespace PriceTracker.Features.TrackedProducts.DTOs
{
    public class TrackedProductDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string Url { get; set; } = null!;

        public Money? CurrentPrice { get; set; }
        public DateTime? LastCheckedAt { get; set; }
    }
}
