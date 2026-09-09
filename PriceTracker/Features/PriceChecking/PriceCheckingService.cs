using PriceTracker.Features.PriceHistory;
using PriceTracker.Features.PriceHistory.ValueObjects;
using PriceTracker.Features.TrackedProducts;

namespace PriceTracker.Features.PriceChecking
{
    public class PriceCheckingService : IPriceCheckingService
    {
        private readonly IPriceScraper _scraper;
        private readonly ITrackedProductService _trackedProductService;
        private readonly IPriceHistoryService _priceHistoryService;
        private readonly ILogger<IPriceCheckingService> _logger;

        public PriceCheckingService(
            IPriceScraper scraper,
            ITrackedProductService trackedProductService,
            IPriceHistoryService priceHistoryService,
            ILogger<IPriceCheckingService> logger)
        {
            _scraper = scraper;
            _trackedProductService = trackedProductService;
            _priceHistoryService = priceHistoryService;
            _logger = logger;
        }

        private async Task<PriceCheckResult> CheckPriceInternal(string url)
        {
            if (!ProductUrlValidator.IsValid(url, out var uri))
            {
                _logger.LogWarning("Price check skipped because product URL is invalid: {Url}", url);
                return PriceCheckResult.InvalidUrl();
            }

            Money? scrapedPrice;
            try
            {
                scrapedPrice = await _scraper.ScrapePriceAsync(uri);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Price scraping failed for {Url}", uri);
                return PriceCheckResult.ScrapeFailed();
            }

            if (scrapedPrice == null)
            {
                _logger.LogWarning("Price scraping returned no price for {Url}", uri);
                return PriceCheckResult.ScrapeFailed();
            }

            return PriceCheckResult.Ok(scrapedPrice.Value);
        }

        public async Task<PriceCheckResult> CheckPriceAsync(Guid id, Guid userId)
        {
            var trackedProduct = await _trackedProductService
                .GetByIdAsync(id, userId);

            if (trackedProduct is null)
                return PriceCheckResult.NotFound();

            var result = await CheckPriceInternal(trackedProduct.Url);

            var checkedAt = await _trackedProductService.UpdateAfterPriceCheckAsync(id);

            if (result.Status == PriceCheckStatus.Success &&
                result.Price != null)
            {
                await _priceHistoryService.AddPriceCheckAsync(
                    trackedProduct.Id,
                    result.Price.Value,
                    checkedAt
                    );

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
