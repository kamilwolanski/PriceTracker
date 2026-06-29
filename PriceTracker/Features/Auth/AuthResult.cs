namespace PriceTracker.Features.Auth
{
    public class AuthResult
    {
        public bool Success { get; set; }
        public string? Token { get; set; }
        public string? RefreshToken { get; set; }

        public string? Error { get; set; }

        public static AuthResult Ok(string token, string refreshToken) =>
            new AuthResult { Success = true, Token = token, RefreshToken = refreshToken };

        public static AuthResult Fail(string error) =>
            new AuthResult { Success = false, Error = error };
    }
}
