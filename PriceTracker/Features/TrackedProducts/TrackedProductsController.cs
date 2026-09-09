using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PriceTracker.Features.PriceChecking;
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
        private readonly ITrackedProductService _trackedProductService;
        private readonly ITrackedProductCreationService _trackedProductCreationService;
        private readonly IPriceCheckingService _priceCheckingService;
        public TrackedProductsController(
            ITrackedProductService trackedProductService,
            ITrackedProductCreationService trackedProductCreationService,
            IPriceCheckingService priceCheckingService
            )
        {
            _trackedProductService = trackedProductService;
            _trackedProductCreationService = trackedProductCreationService;
            _priceCheckingService = priceCheckingService;
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
            var result = await _trackedProductCreationService.CreateAsync(
                product,
                GetUserId());

            if (result.Status == CreateTrackedProductStatus.Success)
            {
                return CreatedAtAction(
                    nameof(GetById),
                    new { id = result.Product!.Id },
                    result.Product
                );
            }

            if (result.Status == CreateTrackedProductStatus.ScrapeFailed)
            {
                return Problem(
                   statusCode: StatusCodes.Status502BadGateway,
                   title: "Price scraping failed",
                   detail: "Could not scrape the product price."
               );
            }

            return StatusCode(StatusCodes.Status500InternalServerError);
        }

        [HttpPost("{id}/check-price")]
        public async Task<IActionResult> CheckPrice(Guid id)
        {
            var result = await _priceCheckingService.CheckTrackedProductPriceAsync(
                id,
                GetUserId());

            if (result.Status == PriceCheckStatus.Success)
            {
                return Ok(result);
            }

            if (result.Status == PriceCheckStatus.ProductNotFound)
            {
                return NotFound(result);
            }

            if (result.Status == PriceCheckStatus.ScrapeFailed)
            {
                return StatusCode(
                    StatusCodes.Status502BadGateway,
                    result);
            }

            if (result.Status == PriceCheckStatus.InvalidUrl)
            {
                return BadRequest(result);
            }

            return StatusCode(StatusCodes.Status500InternalServerError);
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

