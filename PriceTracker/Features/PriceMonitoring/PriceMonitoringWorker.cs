using PriceTracker.Features.PriceChecking;
using PriceTracker.Features.PriceHistory;
using PriceTracker.Features.TrackedProducts;

namespace PriceTracker.Features.PriceMonitoring
{
    public class PriceMonitoringWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public PriceMonitoringWorker(
            IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
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
                            .GetRequiredService<TrackedProductService>();

                    var priceCheckingService =
                        scope.ServiceProvider
                            .GetRequiredService<PriceCheckingService>();

                    var priceHistoryService =
                        scope.ServiceProvider
                            .GetRequiredService<PriceHistoryService>();

                    var skip = 0;
                    const int batchSize = 100;

                    while (true)
                    {
                        var products =
                            await trackedProductService
                                .GetProductsForPriceCheckAsync(
                                    skip,
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
                                    await priceHistoryService.AddPriceCheckAsync(
                                        product.Id,
                                        checkedPrice.Price.Value,
                                        checkedAt);
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(
                                    $"Failed to check product {product.Id}: {ex.Message}");
                            }
                        }

                        skip += batchSize;
                    }
                }

                } catch (Exception ex)
                {
                    Console.WriteLine(
                        $"Price monitoring worker failed: {ex.Message}");
                }

                Console.WriteLine("Price check finished.");

                await Task.Delay(
                    TimeSpan.FromSeconds(10),
                    stoppingToken);
            }
        }
    }
}
