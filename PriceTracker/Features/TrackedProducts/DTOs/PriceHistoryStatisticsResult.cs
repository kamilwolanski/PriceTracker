namespace PriceTracker.Features.TrackedProducts.DTOs
{
    public class PriceHistoryStatisticsResult
    {
        public bool ProductExists { get; set; }
        public PriceHistoryStatistics? Statistics { get; set; }
    }
}
