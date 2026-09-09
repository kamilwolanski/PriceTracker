using PriceTracker.Features.PriceChecking;
using PriceTracker.Features.PriceHistory;
using PriceTracker.Features.TrackedProducts;
using PriceTracker.Features.TrackedProducts.DTOs;
using PriceTracker.Models;

namespace PriceTracker.Features.TrackedProductCreation
{
    public class TrackedProductCreationService : ITrackedProductCreationService
    {
        private readonly IPriceCheckingService _priceCheckingService;
        private readonly ITrackedProductService _trackedProductService;
        private readonly IPriceHistoryService _priceHistoryService;

        public TrackedProductCreationService(ITrackedProductService trackedProductService, IPriceCheckingService priceCheckingService, IPriceHistoryService priceHistoryService)
        {
            _priceCheckingService = priceCheckingService;
            _trackedProductService = trackedProductService;
            _priceHistoryService = priceHistoryService;
        }

        public async Task<CreateTrackedProductResult> CreateAsync(CreateTrackedProductDto dto, Guid userId)
        {
            var checkedPrice = await _priceCheckingService.CheckPriceAsync(dto.Url);
            if (checkedPrice.Status == PriceCheckStatus.Success && checkedPrice.Price != null)
            {
                var price = checkedPrice.Price.Value;

                var newProduct = await _trackedProductService.AddAsync(dto, userId);
                var checkedAt = await _trackedProductService.UpdateAfterPriceCheckAsync(newProduct.Id);
                await _priceHistoryService.AddPriceCheckAsync(
                    newProduct.Id,
                    price,
                    checkedAt
                    );

                return new CreateTrackedProductResult
                {
                    Status = CreateTrackedProductStatus.Success,
                    Product = new TrackedProductDto
                    {
                        Id = newProduct.Id,
                        Name = newProduct.Name,
                        Url = newProduct.Url,
                        CurrentPrice = price,
                        LastCheckedAt = checkedAt
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


