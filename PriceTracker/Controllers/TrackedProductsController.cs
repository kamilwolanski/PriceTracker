using Microsoft.AspNetCore.Mvc;
using PriceTracker.Data;
using PriceTracker.DTOs;
using PriceTracker.Models;

namespace PriceTracker.Controllers
{
    [ApiController]
    [Route("tracked-products")]
    public class TrackedProductsController : ControllerBase
    {
        private readonly AppDbContext _context;
        public TrackedProductsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetTrackedProducts()
        {
            var trackedProducts = _context.TrackedProducts.ToList();
            return Ok(trackedProducts);
        }

        [HttpPost]
        public async Task<IActionResult> AddTrackedProduct([FromBody] CreateTrackedProductDto product)
        {
            var newProduct = new TrackedProduct
            {
                Name = product.Name,
                Url = product.Url
            };

            _context.TrackedProducts.Add(newProduct);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTrackedProducts), new { id = newProduct.Id }, newProduct);

        }
    }
}
