using Microsoft.AspNetCore.Mvc;
using PriceTracker.DTOs.PriceHistory;
using PriceTracker.Services;

namespace PriceTracker.Controllers
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
    }
}
