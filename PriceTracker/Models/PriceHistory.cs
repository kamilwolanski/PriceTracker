namespace PriceTracker.Models
{
    public class PriceHistory
    {
        public Guid Id { get; set; }

        public decimal Price { get; set; }
        public DateTime CheckedAt { get; set; }

        public Guid TrackedProductId { get; set; } 

        public TrackedProduct TrackedProduct { get; set; } = null!;
    }
}
