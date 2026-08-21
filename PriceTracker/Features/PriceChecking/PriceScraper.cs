

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

        public async Task<Money?> ScrapePriceAsync(Uri uri, CancellationToken cancellationToken = default)
        {

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


