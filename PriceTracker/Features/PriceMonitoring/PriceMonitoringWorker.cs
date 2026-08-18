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
                    using var scope = _scopeFactory.CreateScope();

                    var trackedProductService =
                        scope.ServiceProvider
                            .GetRequiredService<TrackedProductService>();

                    var priceCheckingService =
                        scope.ServiceProvider
                            .GetRequiredService<PriceCheckingService>();

                    var priceHistoryService =
                        scope.ServiceProvider
                            .GetRequiredService<PriceHistoryService>();

                    var products =
                        await trackedProductService
                            .GetProductsForPriceCheckAsync();

                    foreach (var product in products)
                    {
                        var result =
                            await priceCheckingService
                                .CheckPriceAsync(product.Url);

                        if (result.Status == PriceCheckStatus.Success &&
                            result.Price != null)
                        {
                            await priceHistoryService.AddPriceCheckAsync(
                                product.Id,
                                result.Price.Value);

                            Console.WriteLine(
                                $"Price checked: {product.Name}");
                        }
                        else
                        {
                            Console.WriteLine(
                                $"Price check failed: {product.Name} - {result.Error}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(
                        $"Price monitoring failed: {ex.Message}");
                }

                await Task.Delay(
                    TimeSpan.FromSeconds(30),
                    stoppingToken);
            }
        }
    }
}
