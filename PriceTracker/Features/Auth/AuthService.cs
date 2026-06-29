using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PriceTracker.Data;
using PriceTracker.Features.Auth.DTOs;
using PriceTracker.Models;

namespace PriceTracker.Features.Auth
{
    public class AuthService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<AuthResult> RegisterAsync(RegisterDto dto)
        {
            var emailTaken = await _context.Users.AnyAsync(u => u.Email == dto.Email);
            if (emailTaken)
                return AuthResult.Fail("Email is already taken.");

            var user = new User
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
            };

            _context.Users.Add(user);
            var refreshToken = GenerateRefreshToken();
            _context.RefreshTokens.Add(new RefreshToken
            {
                Token = refreshToken,
                UserId = user.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
            });
            await _context.SaveChangesAsync();

            return AuthResult.Ok(GenerateToken(user), refreshToken);
        }

        public async Task<AuthResult> LoginAsync(LoginDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null)
                return AuthResult.Fail("Invalid email or password.");

            var passwordValid = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);
            if (!passwordValid)
                return AuthResult.Fail("Invalid email or password.");

            var refreshToken = GenerateRefreshToken();
            _context.RefreshTokens.Add(new RefreshToken
            {
                Token = refreshToken,
                UserId = user.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
            });
            await _context.SaveChangesAsync();

            return AuthResult.Ok(GenerateToken(user), refreshToken);
        }

        public async Task<AuthResult> RefreshTokenAsync(string token)
        {
            var refreshToken = await _context.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == token);

            if (refreshToken == null || refreshToken.ExpiresAt < DateTime.UtcNow)
                return AuthResult.Fail("Invalid or expired refresh token.");

            var newRefreshToken = GenerateRefreshToken();
            _context.RefreshTokens.Remove(refreshToken);
            _context.RefreshTokens.Add(new RefreshToken
            {
                Token = newRefreshToken,
                UserId = refreshToken.UserId,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
            });
            await _context.SaveChangesAsync();
            return AuthResult.Ok(GenerateToken(refreshToken.User), newRefreshToken);
        }

        public async Task<AuthResult> LogoutAsync(string token)
        {
            var refreshToken = await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == token);
            if (refreshToken == null)
                return AuthResult.Fail("Invalid refresh token.");
            _context.RefreshTokens.Remove(refreshToken);
            await _context.SaveChangesAsync();
            return AuthResult.Ok(string.Empty, string.Empty);
        }

        private string GenerateToken(User user)
        {
            var secret = _configuration["Jwt:Secret"]!;
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
            };

            var expiresInMinutes = int.Parse(_configuration["Jwt:ExpiresInMinutes"]!);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiresInMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GenerateRefreshToken()
        {
            var randomBytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }
            return Convert.ToBase64String(randomBytes);
        }
    }
}