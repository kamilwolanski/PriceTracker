using System.ComponentModel.DataAnnotations;

namespace PriceTracker.Features.PriceHistory.DTOs
{
    public class AddPriceHistoryDto
    {
        public Guid TrackedProductId { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Cena musi być większa od 0.")]
        public decimal Price { get; set; }
    }
}
