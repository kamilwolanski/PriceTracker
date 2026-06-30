using PriceTracker.Features.PriceHistory.DTOs;

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
        
        public PriceHistoryDto? History { get; set; }
        public string? Error { get; set; }

        public static PriceCheckResult Ok(PriceHistoryDto history) =>
            new PriceCheckResult { Status = PriceCheckStatus.Success, History = history };

        public static PriceCheckResult NotFound() =>
            new PriceCheckResult { Status = PriceCheckStatus.ProductNotFound, Error = "Product not found." };

        public static PriceCheckResult ScrapeFailed() =>
            new PriceCheckResult { Status = PriceCheckStatus.ScrapeFailed, Error = "Could not scrape price." };
    }
}
