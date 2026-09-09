using PriceTracker.Features.Auth.DTOs;

namespace PriceTracker.Features.Auth
{
    public interface IAuthService
    {
        Task<AuthResult> RegisterAsync(RegisterDto dto);
        Task<AuthResult> LoginAsync(LoginDto dto);
        Task<AuthResult> RefreshTokenAsync(string token);
        Task<AuthResult> LogoutAsync(string token);
    }
}
