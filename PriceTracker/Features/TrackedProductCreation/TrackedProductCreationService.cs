using PriceTracker.Features.PriceChecking;
using PriceTracker.Features.TrackedProducts;
using PriceTracker.Features.TrackedProducts.DTOs;

namespace PriceTracker.Features.TrackedProductCreation
{
    public class TrackedProductCreationService
    {
        private readonly PriceCheckingService _priceCheckingService;
        private readonly TrackedProductService _trackedProductService;
        public TrackedProductCreationService(TrackedProductService trackedProductService, PriceCheckingService priceCheckingService)
        {
            _priceCheckingService = priceCheckingService;
            _trackedProductService = trackedProductService;
        }

        public async Task<CreateTrackedProductResult> CreateAsync(CreateTrackedProductDto dto, Guid userId)
        {
            var newProduct = await _trackedProductService.AddAsync(dto, userId);

            var checkedPrice = await _priceCheckingService.CheckPriceAsync(newProduct.Id, userId);
            if (checkedPrice.Status == PriceCheckStatus.Success)
            {
                return new CreateTrackedProductResult
                {
                    Success = true,
                    Product = new TrackedProductDto
                    {
                        Id = newProduct.Id,
                        Name = newProduct.Name,
                        Url = newProduct.Url,
                        CurrentPrice = checkedPrice.History?.Price,
                        LastCheckedAt = checkedPrice.History?.CheckedAt
                    },
                    InitialPriceChecked = true,
                };
            }

            return new CreateTrackedProductResult
            {
                Success = true,
                Product = newProduct,
                InitialPriceChecked = false,
                Error = checkedPrice.Error
            };
        }
    }
}


