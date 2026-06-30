namespace PriceTracker.Features.PriceChecking
{
    public interface IPriceScraper
    {
        Task<decimal?> ScrapePriceAsync(string url, CancellationToken cancellationToken = default);
    }
}
