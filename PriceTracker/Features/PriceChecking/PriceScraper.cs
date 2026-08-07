

using PriceTracker.Features.PriceHistory.ValueObjects;

namespace PriceTracker.Features.PriceChecking
{
    public class PriceScraper : IPriceScraper
    {
        private readonly IEnumerable<IPriceScrapingStrategy> _strategies;

        public PriceScraper(IEnumerable<IPriceScrapingStrategy> strategies)
        {
            _strategies = strategies;
        }

        public async Task<Money?> ScrapePriceAsync(string url, CancellationToken cancellationToken = default)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
                return null;

            foreach (var strategy in _strategies.OrderBy(strategy => strategy.Priority))
            {
                Console.WriteLine($"Trying {strategy.GetType().Name}");
                if (!strategy.CanHandle(uri))
                    continue;

                var price = await strategy.ScrapePriceAsync(uri, cancellationToken);

                Console.WriteLine($"Result: {price}");
                if (price != null)
                    return price;
            }

            return null;
        }
    }
}


