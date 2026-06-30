namespace PriceTracker.Features.TrackedProducts.DTOs
{
    public class TrackedProductDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string Url { get; set; } = null!;

        public decimal? CurrentPrice { get; set; }
        public DateTime? LastCheckedAt { get; set; }
    }
}
