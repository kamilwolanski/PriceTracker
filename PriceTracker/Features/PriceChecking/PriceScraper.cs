using PriceTracker.Features.PriceChecking.Strategies;

namespace PriceTracker.Features.PriceChecking
{
    public class PriceScraper : IPriceScraper
    {
        private readonly IEnumerable<IPriceScrapingStrategy> _strategies;

        public PriceScraper(IEnumerable<IPriceScrapingStrategy> strategies)
        {
            _strategies = strategies;
        }

        public async Task<decimal?> ScrapePriceAsync(string url, CancellationToken cancellationToken = default)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
                return null;

            foreach (var strategy in _strategies.OrderBy(strategy => strategy.Priority))
            {
                if (!strategy.CanHandle(uri))
                    continue;

                var price = await strategy.ScrapePriceAsync(uri, cancellationToken);
                if (price != null)
                    return price;
            }

            return null;
        }
    }
}
