using Microsoft.AspNetCore.Mvc;
using PriceTracker.Features.TrackedProducts.DTOs;

namespace PriceTracker.Features.TrackedProducts
{
    [ApiController]
    [Route("tracked-products")]
    public class TrackedProductsController : ControllerBase
    {
        private readonly TrackedProductService _trackedProductService;
        public TrackedProductsController(TrackedProductService trackedProductService)
        {
            _trackedProductService = trackedProductService;
        }

        [HttpGet]
        public async Task<IActionResult> GetTrackedProducts()
        {
            var trackedProducts = await _trackedProductService.GetAllTrackedProductsAsync();
            return Ok(trackedProducts);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var product = await _trackedProductService.GetByIdAsync(id);

            if (product == null)
                return NotFound();

            return Ok(product);
        }

        [HttpPost]
        public async Task<IActionResult> AddTrackedProduct([FromBody] CreateTrackedProductDto product)
        {
            var created = await _trackedProductService.AddAsync(product);

            return CreatedAtAction(
                nameof(GetById),
                new { id = created.Id },
                created
            );
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTrackedProduct(Guid id, [FromBody] UpdateTrackedProductDto product)
        {
            var updated = await _trackedProductService.UpdateAsync(id, product);
            if (updated == null)
                return NotFound();
            return Ok(updated);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTrackedProduct(Guid id)
        {
            var deleted = await _trackedProductService.DeleteAsync(id);
            if (!deleted)
                return NotFound();
            return NoContent();
        }
    }
}
