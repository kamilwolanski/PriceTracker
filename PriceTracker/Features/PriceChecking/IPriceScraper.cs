using PriceTracker.Features.PriceHistory.ValueObjects;

namespace PriceTracker.Features.PriceChecking
{
    public interface IPriceScraper
    {
        Task<Money?> ScrapePriceAsync(string url, CancellationToken cancellationToken = default);
    }
}
