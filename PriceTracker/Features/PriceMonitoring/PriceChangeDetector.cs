namespace PriceTracker.Features.PriceMonitoring
{
    public enum PriceChangeType
    {
        Increased,
        Decreased,
        Unchanged
    }
    public class PriceChange
    {
        public decimal PreviousPrice { get; init; }
        public decimal CurrentPrice { get; init; }
        public decimal PercentageChange { get; init; }
        public PriceChangeType Type { get; init; }
    }
    public class PriceChangeDetector
    {
        public PriceChange Detect(decimal previousPrice, decimal currentPrice)
        {

            decimal percentageChange =
                ((currentPrice - previousPrice) / previousPrice) * 100;

            return new PriceChange
            {
                PreviousPrice = previousPrice,
                CurrentPrice = currentPrice,
                PercentageChange = percentageChange,
                Type = percentageChange < 0 ? PriceChangeType.Decreased : percentageChange > 0 ? PriceChangeType.Increased : PriceChangeType.Unchanged
            };
        }
    }
}
