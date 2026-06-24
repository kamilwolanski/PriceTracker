namespace PriceTracker.DTOs.PriceHistory
{
    public class PriceHistoryDto
    {
        public Guid Id { get; set; }
        public decimal Price { get; set; }
        public DateTime CheckedAt { get; set; }
        public Guid TrackedProductId { get; set; }
    }
}
