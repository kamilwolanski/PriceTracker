using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PriceTracker.Features.TrackedProducts.DTOs;

namespace PriceTracker.Features.TrackedProducts
{
    [ApiController]
    [Route("tracked-products")]
    [Authorize]
    public class TrackedProductsController : ControllerBase
    {
        private readonly TrackedProductService _trackedProductService;
        public TrackedProductsController(TrackedProductService trackedProductService)
        {
            _trackedProductService = trackedProductService;
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

        [HttpPost]
        public async Task<IActionResult> AddTrackedProduct([FromBody] CreateTrackedProductDto product)
        {
            var created = await _trackedProductService.AddAsync(product, GetUserId());

            return CreatedAtAction(
                nameof(GetById),
                new { id = created.Id },
                created
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
