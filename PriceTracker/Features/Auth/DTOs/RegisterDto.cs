using System.ComponentModel.DataAnnotations;

namespace PriceTracker.Features.Auth.DTOs
{
    public class RegisterDto
    {
        [Required]
        [MinLength(2)]
        public string Name { get; set; } = null!;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        [MinLength(8)]
        public string Password { get; set; } = null!;
    }
}
