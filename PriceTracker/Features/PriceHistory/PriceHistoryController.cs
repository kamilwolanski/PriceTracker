using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PriceTracker.Features.PriceHistory.DTOs;

namespace PriceTracker.Features.PriceHistory
{
    [ApiController]
    [Route("price-history")]
    [Authorize]
    public class PriceHistoryController : ControllerBase
    {
        private readonly PriceHistoryService _priceHistoryService;
        public PriceHistoryController(PriceHistoryService priceHistoryService)
        {
            _priceHistoryService = priceHistoryService;
        }

        private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var item = await _priceHistoryService.GetByIdAsync(id, GetUserId());

            if (item == null)
                return NotFound();

            return Ok(item);
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] AddPriceHistoryDto dto)
        {
            var created = await _priceHistoryService.AddAsync(dto, GetUserId());

            if (created == null)
                return NotFound();

            return CreatedAtAction(
                nameof(GetById),
                new { id = created.Id },
                created
            );
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePriceHistoryDto dto)
        {
            var updated = await _priceHistoryService.UpdateAsync(id, dto, GetUserId());
            if (updated == null)
                return NotFound();
            return Ok(updated);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await _priceHistoryService.DeleteAsync(id, GetUserId());
            if (!deleted)
                return NotFound();
            return NoContent();
        }
    }
}
