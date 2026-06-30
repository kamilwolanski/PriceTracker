using PriceTracker.Features.PriceHistory;
using PriceTracker.Features.TrackedProducts;

namespace PriceTracker.Features.PriceChecking
{
    public class PriceCheckingService
    {
        private readonly IPriceScraper _scraper;
        private readonly TrackedProductService _trackedProductService;
        private readonly PriceHistoryService _priceHistoryService;

        public PriceCheckingService(
            IPriceScraper scraper,
            TrackedProductService trackedProductService,
            PriceHistoryService priceHistoryService)
        {
            _scraper = scraper;
            _trackedProductService = trackedProductService;
            _priceHistoryService = priceHistoryService;
        }

        public async Task<PriceCheckResult> CheckPriceAsync(Guid trackedProductId, Guid userId)
        {
            var product = await _trackedProductService.GetByIdAsync(trackedProductId, userId);
            if (product == null)
                return PriceCheckResult.NotFound();

            var scrapedPrice = await _scraper.ScrapePriceAsync(product.Url);
            if (scrapedPrice == null)
                return PriceCheckResult.ScrapeFailed();

            var history = await _priceHistoryService.AddFromCheckAsync(
                trackedProductId, scrapedPrice.Value, userId);

            if (history == null)
                return PriceCheckResult.NotFound();

            return PriceCheckResult.Ok(history);
        }
    }
}
