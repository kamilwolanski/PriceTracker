namespace PriceTracker.Features.PriceChecking
{
    public interface IPriceScrapingStrategy
    {
        int Priority { get; }

        bool CanHandle(Uri uri);

        Task<decimal?> ScrapePriceAsync(Uri uri, CancellationToken cancellationToken = default);
    }
}

