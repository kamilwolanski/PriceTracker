namespace PriceTracker.Models
{
    public class TrackedProduct
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string Url { get; set; } = null!;

        public decimal CurrentPrice { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
