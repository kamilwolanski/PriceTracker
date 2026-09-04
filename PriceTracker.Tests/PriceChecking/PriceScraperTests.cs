using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using PriceTracker.Features.PriceChecking;
using PriceTracker.Features.PriceHistory.ValueObjects;

namespace PriceTracker.Tests.PriceChecking
{
    public class PriceScraperTests
    {
        [Fact]
        public async Task ScrapePriceAsync_WithMatchingStrategy_ReturnsPrice()
        {
            var strategy = new Mock<IPriceScrapingStrategy>();

            strategy.Setup(x => x.Priority)
                .Returns(1);

            strategy.Setup(x => x.CanHandle(It.IsAny<Uri>()))
                .Returns(true);

            strategy.Setup(x => x.ScrapePriceAsync(
                    It.IsAny<Uri>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Money(20, "PLN"));

            var scrapingStrategies = new List<IPriceScrapingStrategy>
            {
                strategy.Object
            };

            var scraper = new PriceScraper(
                scrapingStrategies,
                NullLogger<PriceScraper>.Instance);

            var result = await scraper.ScrapePriceAsync(
                new Uri("https://example.com"));

            Assert.NotNull(result);
            Assert.Equal(20m, result.Value.Amount);
        }

        [Fact]
        public async Task ScrapePriceAsync_WithNoMatchingStrategy_ReturnsNull()
        {
            var strategy = new Mock<IPriceScrapingStrategy>();

            strategy.Setup(x => x.Priority)
                .Returns(1);

            strategy.Setup(x => x.CanHandle(It.IsAny<Uri>()))
                .Returns(false);

            var scrapingStrategies = new List<IPriceScrapingStrategy>
            {
                strategy.Object
            };

            var scraper = new PriceScraper(
                scrapingStrategies,
                NullLogger<PriceScraper>.Instance);

            var result = await scraper.ScrapePriceAsync(
                new Uri("https://example.com"));

            Assert.Null(result);

            strategy.Verify(
                x => x.ScrapePriceAsync(
                    It.IsAny<Uri>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task ScrapePriceAsync_WithMatchingStrategyReturningNull_ReturnsNull()
        {
            var strategy = new Mock<IPriceScrapingStrategy>();

            strategy.Setup(x => x.Priority)
                .Returns(1);

            strategy.Setup(x => x.CanHandle(It.IsAny<Uri>()))
                .Returns(true);

            strategy.Setup(x => x.ScrapePriceAsync(
                    It.IsAny<Uri>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((Money?)null);

            var scrapingStrategies = new List<IPriceScrapingStrategy>
            {
                strategy.Object
            };

            var scraper = new PriceScraper(
                scrapingStrategies,
                NullLogger<PriceScraper>.Instance);

            var result = await scraper.ScrapePriceAsync(
                new Uri("https://example.com"));

            Assert.Null(result);
        }

        [Fact]
        public async Task ScrapePriceAsync_WithMultipleStrategies_UsesStrategyWithLowestPriorityValueFirst()
        {
            var strategyWithHighPriority = new Mock<IPriceScrapingStrategy>();

            strategyWithHighPriority.Setup(x => x.Priority)
                .Returns(1);

            strategyWithHighPriority.Setup(x => x.CanHandle(It.IsAny<Uri>()))
                .Returns(true);

            strategyWithHighPriority.Setup(x => x.ScrapePriceAsync(
                    It.IsAny<Uri>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Money(100, "PLN"));


            var strategyWithLowerPriority = new Mock<IPriceScrapingStrategy>();

            strategyWithLowerPriority.Setup(x => x.Priority)
                .Returns(2);

            strategyWithLowerPriority.Setup(x => x.CanHandle(It.IsAny<Uri>()))
                .Returns(true);

            strategyWithLowerPriority.Setup(x => x.ScrapePriceAsync(
                    It.IsAny<Uri>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Money(200, "PLN"));


            var scrapingStrategies = new List<IPriceScrapingStrategy>
            {
                strategyWithLowerPriority.Object,
                strategyWithHighPriority.Object
            };

            var scraper = new PriceScraper(
                scrapingStrategies,
                NullLogger<PriceScraper>.Instance);

            var result = await scraper.ScrapePriceAsync(
                new Uri("https://example.com"));

            Assert.NotNull(result);
            Assert.Equal(100m, result.Value.Amount);

            strategyWithHighPriority.Verify(
                x => x.ScrapePriceAsync(
                    It.IsAny<Uri>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            strategyWithLowerPriority.Verify(
                x => x.ScrapePriceAsync(
                    It.IsAny<Uri>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task ScrapePriceAsync_WithFirstStrategyReturningNull_UsesNextStrategy()
        {
            var strategyWithHighPriority = new Mock<IPriceScrapingStrategy>();

            strategyWithHighPriority.Setup(x => x.Priority)
                .Returns(1);

            strategyWithHighPriority.Setup(x => x.CanHandle(It.IsAny<Uri>()))
                .Returns(true);

            strategyWithHighPriority.Setup(x => x.ScrapePriceAsync(
                    It.IsAny<Uri>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((Money?)null);


            var strategyWithLowerPriority = new Mock<IPriceScrapingStrategy>();

            strategyWithLowerPriority.Setup(x => x.Priority)
                .Returns(2);

            strategyWithLowerPriority.Setup(x => x.CanHandle(It.IsAny<Uri>()))
                .Returns(true);

            strategyWithLowerPriority.Setup(x => x.ScrapePriceAsync(
                    It.IsAny<Uri>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Money(200, "PLN"));


            var scrapingStrategies = new List<IPriceScrapingStrategy>
            {
                strategyWithHighPriority.Object,
                strategyWithLowerPriority.Object
            };

            var scraper = new PriceScraper(
                scrapingStrategies,
                NullLogger<PriceScraper>.Instance);

            var result = await scraper.ScrapePriceAsync(
                new Uri("https://example.com"));

            Assert.NotNull(result);
            Assert.Equal(200m, result.Value.Amount);

            strategyWithHighPriority.Verify(
                x => x.ScrapePriceAsync(
                    It.IsAny<Uri>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            strategyWithLowerPriority.Verify(
                x => x.ScrapePriceAsync(
                    It.IsAny<Uri>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}