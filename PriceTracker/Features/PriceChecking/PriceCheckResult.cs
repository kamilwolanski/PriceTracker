using PriceTracker.Features.PriceHistory.DTOs;
using PriceTracker.Features.PriceHistory.ValueObjects;

namespace PriceTracker.Features.PriceChecking
{
    public enum PriceCheckStatus
    {
        Success,
        ProductNotFound,
        ScrapeFailed
    }
    public class PriceCheckResult
    {
        public PriceCheckStatus Status { get; set; }
        public Money? Price { get; set; }
        public string? Error { get; set; }
        public DateTime? CheckedAt { get; set; }

        public static PriceCheckResult Ok(Money money) =>
            new PriceCheckResult
            {
                Status = PriceCheckStatus.Success,
                Price = money
            };

        public static PriceCheckResult NotFound() =>
            new PriceCheckResult
            {
                Status = PriceCheckStatus.ProductNotFound,
                Error = "Product not found."
            };

        public static PriceCheckResult ScrapeFailed() =>
            new PriceCheckResult
            {
                Status = PriceCheckStatus.ScrapeFailed,
                Error = "Could not scrape price."
            };
    }
}
