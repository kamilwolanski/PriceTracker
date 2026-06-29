namespace PriceTracker.Models
{
    public class User
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public List<TrackedProduct> TrackedProducts { get; set; } = new();
        public List<RefreshToken> RefreshTokens { get; set; } = new();
    }
}
