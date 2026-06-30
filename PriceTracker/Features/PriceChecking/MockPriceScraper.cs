namespace PriceTracker.Features.PriceChecking
{
    public class MockPriceScraper : IPriceScraper
    {
        public Task<decimal?> ScrapePriceAsync(string url, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<decimal?>(123.45m);
        }
    }
}
