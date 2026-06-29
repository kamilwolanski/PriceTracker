using System.ComponentModel.DataAnnotations;

namespace PriceTracker.Features.TrackedProducts.DTOs
{
    public class CreateTrackedProductDto
    {
        [Required]
        [MinLength(2)]
        [MaxLength(200)]
        public string Name { get; set; } = null!;

        [Required]
        [Url]
        public string Url { get; set; } = null!;
    }
}
