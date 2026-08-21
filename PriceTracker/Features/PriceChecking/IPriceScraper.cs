using PriceTracker.Features.PriceHistory.ValueObjects;

namespace PriceTracker.Features.PriceChecking
{
    public interface IPriceScraper
    {
        Task<Money?> ScrapePriceAsync(Uri uri, CancellationToken cancellationToken = default);
    }
}
