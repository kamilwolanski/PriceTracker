namespace PriceTracker.Features.PriceChecking
{
    public interface IPriceCheckingService
    {
        Task<PriceCheckResult> CheckPriceAsync(Guid id, Guid userId);
        Task<PriceCheckResult> CheckPriceAsync(string url);
    }
}
