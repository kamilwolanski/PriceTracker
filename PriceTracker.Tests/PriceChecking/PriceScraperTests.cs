using Microsoft.Extensions.Logging.Abstractions;
using PriceTracker.Features.PriceChecking;
using PriceTracker.Features.PriceHistory.ValueObjects;
using PriceTracker.Tests.PriceChecking.Mocks;

namespace PriceTracker.Tests.PriceChecking
{
    public class PriceScraperTests
    {
        [Fact]
        public async Task ScrapePriceAsync_WithMatchingStrategy_ReturnsPrice()
        {
            IEnumerable<IPriceScrapingStrategy> scrapingStrategies = new List<IPriceScrapingStrategy>
            {
                new MockPriceScrapingStrategy(),
            };

            var scraper = new PriceScraper(scrapingStrategies, NullLogger<PriceScraper>.Instance);

            var url = "https://example.com";
            var result = await scraper.ScrapePriceAsync(new Uri(url));

            Assert.NotNull(result);
            Assert.Equal(20m, result.Value.Amount);
        }

        [Fact]
        public async Task ScrapePriceAsync_WithNoMatchingStrategy_ReturnsNull()
        {
            var strategy = new MockPriceScrapingStrategy
            {
                Handle = false
            };
            IEnumerable<IPriceScrapingStrategy> scrapingStrategies = new List<IPriceScrapingStrategy>
            {
                strategy,
            };

            var scraper = new PriceScraper(scrapingStrategies, NullLogger<PriceScraper>.Instance);

            var url = "https://example.com";
            var result = await scraper.ScrapePriceAsync(new Uri(url));

            Assert.Null(result);
        }

        [Fact]
        public async Task ScrapePriceAsync_WithMatchingStrategyReturningNull_ReturnsNull()
        {
            var strategy = new MockPriceScrapingStrategy
            {
                Handle = true,
                Price = null
            };
            IEnumerable<IPriceScrapingStrategy> scrapingStrategies = new List<IPriceScrapingStrategy>
            {
                strategy,
            };

            var scraper = new PriceScraper(scrapingStrategies, NullLogger<PriceScraper>.Instance);

            var url = "https://example.com";
            var result = await scraper.ScrapePriceAsync(new Uri(url));

            Assert.Null(result);
        }

        [Fact]
        public async Task ScrapePriceAsync_WithMultipleStrategies_UsesStrategyWithLowestPriorityValueFirst()
        {
            var strategyWithHighPriority = new MockPriceScrapingStrategy
            {
                Priority = 1,
                Handle = true,
                Price = new Money(100, "PLN")
            };

            var strategyWithLowerPriority = new MockPriceScrapingStrategy
            {
                Priority = 2,
                Handle = true,
                Price = new Money(200, "PLN")
            };

            IEnumerable<IPriceScrapingStrategy> scrapingStrategies = new List<IPriceScrapingStrategy>
            {
                strategyWithHighPriority,
                strategyWithLowerPriority
            };

            var scraper = new PriceScraper(scrapingStrategies, NullLogger<PriceScraper>.Instance);

            var url = "https://example.com";
            var result = await scraper.ScrapePriceAsync(new Uri(url));

            Assert.NotNull(result);
            Assert.Equal(100m, result.Value.Amount);
        }

        [Fact]
        public async Task ScrapePriceAsync_WithFirstStrategyReturningNull_UsesNextStrategy()
        {
            var strategyWithHighPriority = new MockPriceScrapingStrategy
            {
                Priority = 1,
                Handle = true,
                Price = null
            };

            var strategyWithLowerPriority = new MockPriceScrapingStrategy
            {
                Priority = 2,
                Handle = true,
                Price = new Money(200, "PLN")
            };

            IEnumerable<IPriceScrapingStrategy> scrapingStrategies = new List<IPriceScrapingStrategy>
            {
                strategyWithHighPriority,
                strategyWithLowerPriority
            };

            var scraper = new PriceScraper(scrapingStrategies, NullLogger<PriceScraper>.Instance);

            var url = "https://example.com";
            var result = await scraper.ScrapePriceAsync(new Uri(url));

            Assert.NotNull(result);
            Assert.Equal(200m, result.Value.Amount);
        }
    }
}
