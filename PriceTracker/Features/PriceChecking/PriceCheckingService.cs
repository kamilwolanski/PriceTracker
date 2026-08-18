using PriceTracker.Features.PriceHistory;
using PriceTracker.Features.PriceHistory.ValueObjects;
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

        private async Task<PriceCheckResult> CheckPriceInternal(string url)
        {
            Money? scrapedPrice;
            try
            {
                scrapedPrice = await _scraper.ScrapePriceAsync(url);
            }
            catch
            {
                return PriceCheckResult.ScrapeFailed();
            }

            if (scrapedPrice == null)
                return PriceCheckResult.ScrapeFailed();

            return PriceCheckResult.Ok(scrapedPrice.Value);
        }

        public async Task<PriceCheckResult> CheckPriceAsync(Guid id, Guid userId)
        {
            var trackedProduct = await _trackedProductService
                .GetByIdAsync(id, userId);

            if (trackedProduct is null)
                return PriceCheckResult.NotFound();

            var result = await CheckPriceInternal(trackedProduct.Url);

            if (result.Status == PriceCheckStatus.Success &&
                result.Price != null)
            {
                var checkedAt = await _priceHistoryService.AddPriceCheckAsync(
                    trackedProduct.Id,
                    result.Price.Value);

                result.CheckedAt = checkedAt;
            }

            return result;
        }

        public async Task<PriceCheckResult> CheckPriceAsync(string url)
        {
            return await CheckPriceInternal(url);
        }
    }
}

