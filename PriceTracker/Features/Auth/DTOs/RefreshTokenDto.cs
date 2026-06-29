using System.ComponentModel.DataAnnotations;

namespace PriceTracker.Features.Auth.DTOs
{
    public class RefreshTokenDto
    {
        [Required]
        public string Token { get; set; } = null!;
    }
}
