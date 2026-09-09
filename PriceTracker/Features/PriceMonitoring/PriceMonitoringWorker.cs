using PriceTracker.Features.NotificationService;
using PriceTracker.Features.PriceChecking;
using PriceTracker.Features.PriceHistory;
using PriceTracker.Features.TrackedProducts;

namespace PriceTracker.Features.PriceMonitoring
{
    public class PriceMonitoringWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<PriceMonitoringWorker> _logger;
        private readonly PriceChangeDetector _priceChangeDetector;
        private readonly INotificationService _notificationService;

        public PriceMonitoringWorker(
            IServiceScopeFactory scopeFactory,
            ILogger<PriceMonitoringWorker> logger,
            PriceChangeDetector priceChangeDetector,
            INotificationService notificationService
            )
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _priceChangeDetector = priceChangeDetector;
            _notificationService = notificationService;
        }

        protected override async Task ExecuteAsync(
            CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var trackedProductService =
                            scope.ServiceProvider
                                .GetRequiredService<ITrackedProductService>();

                        var priceCheckingService =
                            scope.ServiceProvider
                                .GetRequiredService<IPriceCheckingService>();

                        var priceHistoryService =
                            scope.ServiceProvider
                                .GetRequiredService<IPriceHistoryService>();

                        const int batchSize = 100;

                        while (true)
                        {
                            var products =
                                await trackedProductService
                                    .GetProductsForPriceCheckAsync(
                                        0,
                                        batchSize);

                            if (products.Count == 0)
                                break;

                            foreach (var product in products)
                            {
                                try
                                {
                                    var checkedPrice =
                                        await priceCheckingService.CheckPriceAsync(product.Url);

                                    var checkedAt =
                                        await trackedProductService.UpdateAfterPriceCheckAsync(product.Id);

                                    if (checkedPrice.Status == PriceCheckStatus.Success &&
                                        checkedPrice.Price != null)
                                    {
                                        var lastPrice = await priceHistoryService.GetTheLastPriceAsync(product.Id);

                                        await priceHistoryService.AddPriceCheckAsync(
                                            product.Id,
                                            checkedPrice.Price.Value,
                                            checkedAt);

                                        if(lastPrice != null)
                                        {
                                            var priceChange = _priceChangeDetector.Detect(lastPrice.Price.Amount, checkedPrice.Price.Value.Amount);

                                            if(priceChange.Type == PriceChangeType.Decreased && priceChange.PercentageChange <= -10)
                                            {
                                               await _notificationService.NotifyPriceDropAsync(priceChange);
                                            }
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogError(
                                        ex,
                                        "Failed to check price for tracked product {TrackedProductId}",
                                        product.Id);
                                }
                            }

                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Price monitoring worker failed");
                }

                _logger.LogInformation("Price check finished");

                await Task.Delay(
                    TimeSpan.FromSeconds(10),
                    stoppingToken);
            }
        }
    }
}
