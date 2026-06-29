namespace PriceTracker.Models
{
    public class TrackedProduct
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string Url { get; set; } = null!;
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
        public List<PriceHistory> PriceHistory { get; set; } = new();
    }
}
