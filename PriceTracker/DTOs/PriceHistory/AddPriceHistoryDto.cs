namespace PriceTracker.DTOs.PriceHistory
{
    public class AddPriceHistoryDto
    {
        public Guid TrackedProductId { get; set; }
        public decimal Price { get; set; }
    }
}
