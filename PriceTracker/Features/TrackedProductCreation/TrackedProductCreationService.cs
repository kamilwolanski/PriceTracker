using PriceTracker.Features.PriceChecking;
using PriceTracker.Features.PriceHistory;
using PriceTracker.Features.TrackedProducts;
using PriceTracker.Features.TrackedProducts.DTOs;
using PriceTracker.Models;

namespace PriceTracker.Features.TrackedProductCreation
{
    public class TrackedProductCreationService
    {
        private readonly PriceCheckingService _priceCheckingService;
        private readonly TrackedProductService _trackedProductService;
        private readonly PriceHistoryService _priceHistoryService;
        public TrackedProductCreationService(TrackedProductService trackedProductService, PriceCheckingService priceCheckingService, PriceHistoryService priceHistoryService)
        {
            _priceCheckingService = priceCheckingService;
            _trackedProductService = trackedProductService;
            _priceHistoryService = priceHistoryService;
        }

        public async Task<CreateTrackedProductResult> CreateAsync(CreateTrackedProductDto dto, Guid userId)
        {
            var checkedPrice = await _priceCheckingService.CheckPriceAsync(dto.Url);
            if (checkedPrice.Status == PriceCheckStatus.Success && checkedPrice.Money != null)
            {
                var price = checkedPrice.Money.Value;
                var newProduct = await _trackedProductService.AddAsync(dto, userId);
                var history = await _priceHistoryService.AddFromCheckAsync(newProduct.Id, price);

                return new CreateTrackedProductResult
                {
                    Status = CreateTrackedProductStatus.Success,
                    Product = new TrackedProductDto
                    {
                        Id = newProduct.Id,
                        Name = newProduct.Name,
                        Url = newProduct.Url,
                        CurrentPrice = price,
                        LastCheckedAt = history.CheckedAt,
                    },
                };
            }

            return new CreateTrackedProductResult
            {
                Status = CreateTrackedProductStatus.ScrapeFailed,
            };
        }
    }
}


