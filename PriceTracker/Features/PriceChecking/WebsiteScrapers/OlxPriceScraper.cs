using PriceTracker.Features.PriceHistory.ValueObjects;


namespace PriceTracker.Features.PriceChecking.WebsiteScrapers
{
    public class OlxPriceScraper : IPriceScrapingStrategy
    {
        public int Priority => 100;

        public bool CanHandle(Uri uri)
        {
            return uri.Host.EndsWith("olx.pl", StringComparison.OrdinalIgnoreCase);
        }

        public Task<Money?> ScrapePriceAsync(Uri uri, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<Money?>(null);
        }
    }
}


