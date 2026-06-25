namespace PriceTracker.Features.PriceHistory.DTOs
{
    public class AddPriceHistoryDto
    {
        public Guid TrackedProductId { get; set; }
        public decimal Price { get; set; }
    }
}
