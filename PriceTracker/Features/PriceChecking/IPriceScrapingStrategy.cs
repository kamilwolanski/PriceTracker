using PriceTracker.Features.PriceHistory.ValueObjects;

namespace PriceTracker.Features.PriceChecking
{
    public interface IPriceScrapingStrategy
    {
        int Priority { get; }

        bool CanHandle(Uri uri);

        Task<Money?> ScrapePriceAsync(Uri uri, CancellationToken cancellationToken = default);
    }
}

