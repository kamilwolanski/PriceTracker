using PriceTracker.Features.PriceChecking.Strategies;

namespace PriceTracker.Features.PriceChecking.WebsiteScrapers
{
    public class XkomPriceScraper : IPriceScrapingStrategy
    {
        public int Priority => 100;

        public bool CanHandle(Uri uri)
        {
            return uri.Host.EndsWith("x-kom.pl", StringComparison.OrdinalIgnoreCase);
        }

        public Task<decimal?> ScrapePriceAsync(Uri uri, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<decimal?>(null);
        }
    }
}
