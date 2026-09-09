using Microsoft.Extensions.Logging;
using Moq;
using PriceTracker.Data;
using PriceTracker.Features.PriceChecking;
using PriceTracker.Features.PriceHistory;
using PriceTracker.Features.PriceHistory.ValueObjects;
using PriceTracker.Features.TrackedProducts;
using PriceTracker.Features.TrackedProducts.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace PriceTracker.Tests.PriceChecking
{
    public class PriceCheckingServiceTests
    {
        [Fact]
        public async Task CheckPriceAsync_WithValidUrlAndScrapedPrice_ReturnsSuccess()
        {
            var validUrl = "https://example.com";

            var scraper = new Mock<IPriceScraper>();
            scraper.Setup(x => x.ScrapePriceAsync(
                It.IsAny<Uri>()
                )).ReturnsAsync(new Money(200, "PLN"));

            var trackedProductService = new Mock<ITrackedProductService>();

            var priceHistoryService = new Mock<IPriceHistoryService>();

            var logger = new Mock<ILogger<PriceCheckingService>>();

            var priceCheckingService = new PriceCheckingService(
                scraper.Object,
                trackedProductService.Object,
                priceHistoryService.Object,
                logger.Object
                );

            var result = await priceCheckingService.CheckPriceAsync(validUrl);

            Assert.Equal(PriceCheckStatus.Success, result.Status);
            Assert.Equal(200, result.Price.Value.Amount);
            Assert.Equal("PLN", result.Price.Value.CurrencyCode);
        }

        [Fact]
        public async Task CheckPriceAsync_WithInvalidUrl_ReturnsInvalidUrl()
        {
            var invalidUrl = "httpssx://example..com";
            var scraper = new Mock<IPriceScraper>();

            var trackedProductService = new Mock<ITrackedProductService>();

            var priceHistoryService = new Mock<IPriceHistoryService>();

            var logger = new Mock<ILogger<PriceCheckingService>>();

            var priceCheckingService = new PriceCheckingService(
                scraper.Object,
                trackedProductService.Object,
                priceHistoryService.Object,
                logger.Object
                );

            var result = await priceCheckingService.CheckPriceAsync(invalidUrl);
            Assert.Equal(PriceCheckStatus.InvalidUrl, result.Status);
            scraper.Verify(x => x.ScrapePriceAsync(
                It.IsAny<Uri>()
                ), Times.Never);
        }

        [Fact]
        public async Task CheckPriceAsync_WithNoScrapedPrice_ReturnsScrapeFailed()
        {
            var validUrl = "https://example.com";

            var scraper = new Mock<IPriceScraper>();
            scraper.Setup(x => x.ScrapePriceAsync(
                It.IsAny<Uri>()
                )).ReturnsAsync((Money?)null);

            var trackedProductService = new Mock<ITrackedProductService>();

            var priceHistoryService = new Mock<IPriceHistoryService>();

            var logger = new Mock<ILogger<PriceCheckingService>>();

            var priceCheckingService = new PriceCheckingService(
                scraper.Object,
                trackedProductService.Object,
                priceHistoryService.Object,
                logger.Object
                );

            var result = await priceCheckingService.CheckPriceAsync(validUrl);

            Assert.Equal(PriceCheckStatus.ScrapeFailed, result.Status);
        }

        [Fact]
        public async Task CheckPriceAsync_WhenScraperThrows_ReturnsScrapeFailed()
        {
            var validUrl = "https://example.com";

            var scraper = new Mock<IPriceScraper>();
            scraper.Setup(x => x.ScrapePriceAsync(
                It.IsAny<Uri>()
                )).ThrowsAsync(new Exception());
            var trackedProductService = new Mock<ITrackedProductService>();

            var priceHistoryService = new Mock<IPriceHistoryService>();

            var logger = new Mock<ILogger<PriceCheckingService>>();

            var priceCheckingService = new PriceCheckingService(
                scraper.Object,
                trackedProductService.Object,
                priceHistoryService.Object,
                logger.Object
                );

            var result = await priceCheckingService.CheckPriceAsync(validUrl);

            Assert.Equal(PriceCheckStatus.ScrapeFailed, result.Status);
        }

        [Fact]
        public async Task CheckPriceAsync_WhenProductDoesNotExist_ReturnsNotFound()
        {
            var id = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var scraper = new Mock<IPriceScraper>();

            var trackedProductService = new Mock<ITrackedProductService>();
            trackedProductService.Setup(x => x.GetByIdAsync(id, userId)).ReturnsAsync((TrackedProductDto?)null);

            var priceHistoryService = new Mock<IPriceHistoryService>();

            var logger = new Mock<ILogger<PriceCheckingService>>();

            var priceCheckingService = new PriceCheckingService(
                scraper.Object,
                trackedProductService.Object,
                priceHistoryService.Object,
                logger.Object
                );
            var result = await priceCheckingService.CheckTrackedProductPriceAsync(id, userId);
            Assert.Equal(PriceCheckStatus.ProductNotFound, result.Status);
            scraper.Verify(x => x.ScrapePriceAsync(It.IsAny<Uri>()), Times.Never);

        }

        [Fact]
        public async Task CheckPriceAsync_WhenPriceCheckSucceeds_AddsPriceHistory()
        {
            var id = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var trackedProductDto = new TrackedProductDto { 
                Id = id, 
                Name = "Test",
                Url = "https://example.com",
                CurrentPrice = new Money(200, "PLN")
            };

            var scraper = new Mock<IPriceScraper>();
            scraper
            .Setup(x => x.ScrapePriceAsync(
                It.IsAny<Uri>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Money(210m, "PLN"));

            var trackedProductService = new Mock<ITrackedProductService>();
            trackedProductService.Setup(x => x.GetByIdAsync(id, userId)).ReturnsAsync(trackedProductDto);
            var checkedAt = new DateTime(2026, 9, 4, 12, 0, 0);

            trackedProductService
                .Setup(x => x.UpdateAfterPriceCheckAsync(id))
                .ReturnsAsync(checkedAt);

            var priceHistoryService = new Mock<IPriceHistoryService>();

            var logger = new Mock<ILogger<PriceCheckingService>>();

            var priceCheckingService = new PriceCheckingService(
                scraper.Object,
                trackedProductService.Object,
                priceHistoryService.Object,
                logger.Object
                );

            var result = await priceCheckingService.CheckTrackedProductPriceAsync(id, userId);

            Assert.Equal(PriceCheckStatus.Success, result.Status);
            Assert.Equal(210, result.Price.Value.Amount);
            Assert.Equal("PLN", result.Price.Value.CurrencyCode);
            priceHistoryService.Verify(x => x.AddPriceCheckAsync(id, result.Price.Value, result.CheckedAt.Value), Times.Once);
        }

        [Fact]
        public async Task CheckPriceAsync_WhenScrapingReturnsNull_ReturnsScrapeFailed()
        {
            var id = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var trackedProductDto = new TrackedProductDto
            {
                Id = id,
                Name = "Test",
                Url = "https://example.com",
                CurrentPrice = new Money(200, "PLN")
            };

            var scraper = new Mock<IPriceScraper>();
            scraper
            .Setup(x => x.ScrapePriceAsync(
                It.IsAny<Uri>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Money?)null);

            var trackedProductService = new Mock<ITrackedProductService>();
            trackedProductService.Setup(x => x.GetByIdAsync(id, userId)).ReturnsAsync(trackedProductDto);
            var checkedAt = new DateTime(2026, 9, 4, 12, 0, 0);

            trackedProductService
                .Setup(x => x.UpdateAfterPriceCheckAsync(id))
                .ReturnsAsync(checkedAt);

            var priceHistoryService = new Mock<IPriceHistoryService>();

            var logger = new Mock<ILogger<PriceCheckingService>>();

            var priceCheckingService = new PriceCheckingService(
                scraper.Object,
                trackedProductService.Object,
                priceHistoryService.Object,
                logger.Object
                );

            var result = await priceCheckingService.CheckTrackedProductPriceAsync(id, userId);

            Assert.Equal(PriceCheckStatus.ScrapeFailed, result.Status);
            trackedProductService.Verify(x => x.UpdateAfterPriceCheckAsync(id), Times.Once);
            priceHistoryService.Verify(
                x => x.AddPriceCheckAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<Money>(),
                    It.IsAny<DateTime>()),
                Times.Never);

        }

    }
}
