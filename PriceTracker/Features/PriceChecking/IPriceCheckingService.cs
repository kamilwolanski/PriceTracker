namespace PriceTracker.Features.PriceChecking
{
    public interface IPriceCheckingService
    {
        Task<PriceCheckResult> CheckTrackedProductPriceAsync(Guid id, Guid userId);
        Task<PriceCheckResult> CheckPriceAsync(string url);
    }
}
