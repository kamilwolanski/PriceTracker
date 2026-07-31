using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PriceTracker.Features.TrackedProductCreation;
using PriceTracker.Features.TrackedProducts.DTOs;
using System.Security.Claims;

namespace PriceTracker.Features.TrackedProducts
{
    [ApiController]
    [Route("tracked-products")]
    [Authorize]
    public class TrackedProductsController : ControllerBase
    {
        private readonly TrackedProductService _trackedProductService;
        private readonly TrackedProductCreationService _trackedProductCreationService;
        public TrackedProductsController(
            TrackedProductService trackedProductService,
            TrackedProductCreationService trackedProductCreationService)
        {
            _trackedProductService = trackedProductService;
            _trackedProductCreationService = trackedProductCreationService;
        }

        private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet]
        public async Task<IActionResult> GetTrackedProducts()
        {
            var trackedProducts = await _trackedProductService.GetAllTrackedProductsAsync(GetUserId());
            return Ok(trackedProducts);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var product = await _trackedProductService.GetByIdAsync(id, GetUserId());

            if (product == null)
                return NotFound();

            return Ok(product);
        }

        [HttpGet("{id}/price-history")]
        public async Task<IActionResult> GetPriceHistory(Guid id)
        {
            var priceHistory = await _trackedProductService.GetPriceHistoryAsync(id, GetUserId());
            if (priceHistory == null)
                return NotFound();
            return Ok(priceHistory);
        }

        [HttpPost]
        public async Task<IActionResult> AddTrackedProduct([FromBody] CreateTrackedProductDto product)
        {
            var result = await _trackedProductCreationService.CreateAsync(product, GetUserId());

            return CreatedAtAction(
                nameof(GetById),
                new { id = result.Product!.Id },
                result
            );
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTrackedProduct(Guid id, [FromBody] UpdateTrackedProductDto product)
        {
            var updated = await _trackedProductService.UpdateAsync(id, product, GetUserId());
            if (updated == null)
                return NotFound();
            return Ok(updated);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTrackedProduct(Guid id)
        {
            var deleted = await _trackedProductService.DeleteAsync(id, GetUserId());
            if (!deleted)
                return NotFound();
            return NoContent();
        }
    }
}

