using PriceTracker.Features.PriceMonitoring;

namespace PriceTracker.Features.NotificationService
{
    public interface INotificationService
    {
        Task NotifyPriceDropAsync(PriceChange priceChange);
    }
}
