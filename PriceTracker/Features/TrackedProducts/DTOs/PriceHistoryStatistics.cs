namespace PriceTracker.Features.TrackedProducts.DTOs
{
    public class PriceHistoryStatistics
    {
        public decimal Min { get; init; }
        public decimal Max { get; init; }
        public decimal Average { get; init; }
    }
}
