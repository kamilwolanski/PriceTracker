using System.Net;
using System.Net.Http.Json;

namespace PriceTracker.Tests.Auth
{
    public class AuthControllerTests : IClassFixture<TestWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public AuthControllerTests(TestWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        // ── Register ──────────────────────────────────────────────────────────

        [Fact]
        public async Task Register_WithValidData_ReturnsOkWithTokens()
        {
            var response = await _client.PostAsJsonAsync("/auth/register", new
            {
                Name = "Jan Kowalski",
                Email = "jan@test.com",
                Password = "Secret123!"
            });

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadFromJsonAsync<TokenResponse>();
            Assert.NotNull(body);
            Assert.NotEmpty(body.Token);
            Assert.NotEmpty(body.RefreshToken);
        }

        [Fact]
        public async Task Register_WithDuplicateEmail_ReturnsConflict()
        {
            var dto = new { Name = "Jan", Email = "duplicate@test.com", Password = "Secret123!" };
            await _client.PostAsJsonAsync("/auth/register", dto);

            var response = await _client.PostAsJsonAsync("/auth/register", dto);

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }

        [Fact]
        public async Task Register_WithInvalidEmail_ReturnsBadRequest()
        {
            var response = await _client.PostAsJsonAsync("/auth/register", new
            {
                Name = "Jan",
                Email = "niejestemmail",
                Password = "Secret123!"
            });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Register_WithShortPassword_ReturnsBadRequest()
        {
            var response = await _client.PostAsJsonAsync("/auth/register", new
            {
                Name = "Jan",
                Email = "jan2@test.com",
                Password = "123"
            });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        // ── Login ─────────────────────────────────────────────────────────────

        [Fact]
        public async Task Login_WithValidCredentials_ReturnsOkWithTokens()
        {
            await _client.PostAsJsonAsync("/auth/register", new
            {
                Name = "Anna",
                Email = "anna@test.com",
                Password = "Secret123!"
            });

            var response = await _client.PostAsJsonAsync("/auth/login", new
            {
                Email = "anna@test.com",
                Password = "Secret123!"
            });

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadFromJsonAsync<TokenResponse>();
            Assert.NotNull(body);
            Assert.NotEmpty(body.Token);
            Assert.NotEmpty(body.RefreshToken);
        }

        [Fact]
        public async Task Login_WithWrongPassword_ReturnsUnauthorized()
        {
            await _client.PostAsJsonAsync("/auth/register", new
            {
                Name = "Piotr",
                Email = "piotr@test.com",
                Password = "Secret123!"
            });

            var response = await _client.PostAsJsonAsync("/auth/login", new
            {
                Email = "piotr@test.com",
                Password = "ZleHaslo999!"
            });

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Login_WithNonExistentEmail_ReturnsUnauthorized()
        {
            var response = await _client.PostAsJsonAsync("/auth/login", new
            {
                Email = "nieistnieje@test.com",
                Password = "Secret123!"
            });

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        // ── Refresh ───────────────────────────────────────────────────────────

        [Fact]
        public async Task Refresh_WithValidToken_ReturnsNewTokens()
        {
            var registerResponse = await _client.PostAsJsonAsync("/auth/register", new
            {
                Name = "Maria",
                Email = "maria@test.com",
                Password = "Secret123!"
            });
            var tokens = await registerResponse.Content.ReadFromJsonAsync<TokenResponse>();

            var response = await _client.PostAsJsonAsync("/auth/refresh", new
            {
                Token = tokens!.RefreshToken
            });

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var newTokens = await response.Content.ReadFromJsonAsync<TokenResponse>();
            Assert.NotNull(newTokens);
            Assert.NotEmpty(newTokens.Token);
            Assert.NotEmpty(newTokens.RefreshToken);
            Assert.NotEqual(tokens.RefreshToken, newTokens.RefreshToken);
        }

        [Fact]
        public async Task Refresh_WithInvalidToken_ReturnsUnauthorized()
        {
            var response = await _client.PostAsJsonAsync("/auth/refresh", new
            {
                Token = "nieistniejacy-token"
            });

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Refresh_TokenRotation_OldTokenIsInvalid()
        {
            var registerResponse = await _client.PostAsJsonAsync("/auth/register", new
            {
                Name = "Karol",
                Email = "karol@test.com",
                Password = "Secret123!"
            });
            var tokens = await registerResponse.Content.ReadFromJsonAsync<TokenResponse>();

            // Użyj refresh tokenu — dostaniesz nowy
            await _client.PostAsJsonAsync("/auth/refresh", new { Token = tokens!.RefreshToken });

            // Spróbuj użyć starego refresh tokenu ponownie
            var response = await _client.PostAsJsonAsync("/auth/refresh", new
            {
                Token = tokens.RefreshToken
            });

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        // ── Logout ────────────────────────────────────────────────────────────

        [Fact]
        public async Task Logout_WithValidToken_ReturnsNoContent()
        {
            var registerResponse = await _client.PostAsJsonAsync("/auth/register", new
            {
                Name = "Ewa",
                Email = "ewa@test.com",
                Password = "Secret123!"
            });
            var tokens = await registerResponse.Content.ReadFromJsonAsync<TokenResponse>();

            var response = await _client.PostAsJsonAsync("/auth/logout", new
            {
                Token = tokens!.RefreshToken
            });

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task Logout_RefreshTokenIsInvalidAfterLogout()
        {
            var registerResponse = await _client.PostAsJsonAsync("/auth/register", new
            {
                Name = "Tomek",
                Email = "tomek@test.com",
                Password = "Secret123!"
            });
            var tokens = await registerResponse.Content.ReadFromJsonAsync<TokenResponse>();

            await _client.PostAsJsonAsync("/auth/logout", new { Token = tokens!.RefreshToken });

            // Próba użycia refresh tokenu po wylogowaniu
            var response = await _client.PostAsJsonAsync("/auth/refresh", new
            {
                Token = tokens.RefreshToken
            });

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        // ── Helper ────────────────────────────────────────────────────────────

        private record TokenResponse(string Token, string RefreshToken);
    }
}
