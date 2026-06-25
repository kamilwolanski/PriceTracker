using Microsoft.AspNetCore.Mvc;
using PriceTracker.Features.PriceHistory.DTOs;

namespace PriceTracker.Features.PriceHistory
{
    [ApiController]
    [Route("price-history")]
    public class PriceHistoryController : ControllerBase
    {
        private readonly PriceHistoryService _priceHistoryService;
        public PriceHistoryController(PriceHistoryService priceHistoryService)
        {
            _priceHistoryService = priceHistoryService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var item = await _priceHistoryService.GetByIdAsync(id);

            if (item == null)
                return NotFound();

            return Ok(item);
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] AddPriceHistoryDto dto)
        {
            var created = await _priceHistoryService.AddAsync(dto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = created.Id },
                created
            );
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePriceHistoryDto dto)
        {
            var updated = await _priceHistoryService.UpdateAsync(id, dto);
            if (updated == null)
                return NotFound();
            return Ok(updated);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await _priceHistoryService.DeleteAsync(id);
            if (!deleted)
                return NotFound();
            return NoContent();
        }
    }
}
