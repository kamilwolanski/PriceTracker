using Microsoft.AspNetCore.Mvc;
using PriceTracker.Features.Auth.DTOs;

namespace PriceTracker.Features.Auth
{
    [ApiController]
    [Route("auth")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            var result = await _authService.RegisterAsync(dto);
            if (!result.Success)
                return Conflict(result.Error);

            return Ok(new { result.Token, result.RefreshToken });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var result = await _authService.LoginAsync(dto);
            if (!result.Success)
                return Unauthorized(result.Error);

            return Ok(new { result.Token, result.RefreshToken });
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenDto dto)
        {
            var result = await _authService.RefreshTokenAsync(dto.Token);
            if (!result.Success)
                return Unauthorized(result.Error);
            return Ok(new { result.Token, result.RefreshToken });
        }


        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] RefreshTokenDto dto)
        {
            var result = await _authService.LogoutAsync(dto.Token);
            if (!result.Success)
                return BadRequest(result.Error);
            return NoContent();
        }
    }
}
