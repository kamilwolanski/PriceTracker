using PriceTracker.Features.PriceHistory.ValueObjects;

namespace PriceTracker.Features.PriceChecking
{
    public class PriceScraper : IPriceScraper
    {
        private readonly ILogger<PriceScraper> _logger;
        private readonly IEnumerable<IPriceScrapingStrategy> _strategies;

        public PriceScraper(
            IEnumerable<IPriceScrapingStrategy> strategies,
            ILogger<PriceScraper> logger)
        {
            _strategies = strategies;
            _logger = logger;
        }

        public async Task<Money?> ScrapePriceAsync(Uri uri, CancellationToken cancellationToken = default)
        {
            foreach (var strategy in _strategies.OrderBy(strategy => strategy.Priority))
            {
                if (!strategy.CanHandle(uri))
                    continue;

                _logger.LogDebug(
                    "Trying price scraping strategy {StrategyName} for {Url}",
                    strategy.GetType().Name,
                    uri);

                var price = await strategy.ScrapePriceAsync(uri, cancellationToken);

                if (price != null)
                {
                    _logger.LogInformation(
                        "Price scraped successfully using {StrategyName} for {Url}",
                        strategy.GetType().Name,
                        uri);

                    return price;
                }
            }

            _logger.LogWarning("No price scraping strategy returned a price for {Url}", uri);

            return null;
        }
    }
}


