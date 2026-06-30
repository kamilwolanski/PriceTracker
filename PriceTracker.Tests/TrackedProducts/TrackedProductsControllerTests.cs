using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace PriceTracker.Tests.TrackedProducts
{
    public class TrackedProductsControllerTests : IClassFixture<TestWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public TrackedProductsControllerTests(TestWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        // ── Authorization ─────────────────────────────────────────────────────

        [Fact]
        public async Task GetTrackedProducts_WithoutToken_ReturnsUnauthorized()
        {
            var response = await _client.GetAsync("/tracked-products");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task GetTrackedProducts_WithValidToken_ReturnsOk()
        {
            var token = await RegisterAndGetToken("user1@test.com");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _client.GetAsync("/tracked-products");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        // ── CRUD ──────────────────────────────────────────────────────────────

        [Fact]
        public async Task AddTrackedProduct_WithValidData_ReturnsCreated()
        {
            var token = await RegisterAndGetToken("user2@test.com");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _client.PostAsJsonAsync("/tracked-products", new
            {
                Name = "Laptop Dell",
                Url = "https://example.com/laptop"
            });

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task GetTrackedProducts_ReturnsOnlyOwnProducts()
        {
            // User A dodaje produkt
            var tokenA = await RegisterAndGetToken("userA@test.com");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenA);
            await _client.PostAsJsonAsync("/tracked-products", new
            {
                Name = "Produkt A",
                Url = "https://example.com/a"
            });

            // User B dodaje produkt
            var tokenB = await RegisterAndGetToken("userB@test.com");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenB);
            await _client.PostAsJsonAsync("/tracked-products", new
            {
                Name = "Produkt B",
                Url = "https://example.com/b"
            });

            // User B widzi tylko swój produkt
            var response = await _client.GetAsync("/tracked-products");
            var products = await response.Content.ReadFromJsonAsync<List<ProductResponse>>();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Single(products!);
            Assert.Equal("Produkt B", products![0].Name);
        }

        [Fact]
        public async Task GetTrackedProductHistory_ReturnsHistoryForOwnProduct()
        {
            var token = await RegisterAndGetToken("history-user@test.com");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var createResponse = await _client.PostAsJsonAsync("/tracked-products", new
            {
                Name = "Produkt A",
                Url = "https://example.com/a"
            });
            var created = await createResponse.Content.ReadFromJsonAsync<ProductResponse>();

            // dodaj wpis historii
            await _client.PostAsJsonAsync("/price-history", new
            {
                TrackedProductId = created!.Id,
                Price = 99.99m
            });

            var response = await _client.GetAsync($"/tracked-products/{created.Id}/price-history");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var history = await response.Content.ReadFromJsonAsync<List<PriceHistoryResponse>>();
            Assert.Single(history!);
            Assert.Equal(99.99m, history![0].Price);
        }

        private record PriceHistoryResponse(Guid Id, decimal Price, DateTime CheckedAt, Guid TrackedProductId);

        [Fact]
        public async Task GetById_OtherUsersProduct_ReturnsNotFound()
        {
            // User A tworzy produkt
            var tokenA = await RegisterAndGetToken("ownerA@test.com");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenA);
            var createResponse = await _client.PostAsJsonAsync("/tracked-products", new
            {
                Name = "Produkt A",
                Url = "https://example.com/a"
            });
            var created = await createResponse.Content.ReadFromJsonAsync<ProductResponse>();

            // User B próbuje pobrać produkt Usera A
            var tokenB = await RegisterAndGetToken("ownerB@test.com");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenB);

            var response = await _client.GetAsync($"/tracked-products/{created!.Id}");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task DeleteTrackedProduct_OwnProduct_ReturnsNoContent()
        {
            var token = await RegisterAndGetToken("deleter@test.com");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var createResponse = await _client.PostAsJsonAsync("/tracked-products", new
            {
                Name = "Do usunięcia",
                Url = "https://example.com/delete"
            });
            var created = await createResponse.Content.ReadFromJsonAsync<ProductResponse>();

            var response = await _client.DeleteAsync($"/tracked-products/{created!.Id}");

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private async Task<string> RegisterAndGetToken(string email)
        {
            var response = await _client.PostAsJsonAsync("/auth/register", new
            {
                Name = "Test User",
                Email = email,
                Password = "Secret123!"
            });
            var body = await response.Content.ReadFromJsonAsync<TokenResponse>();
            return body!.Token;
        }

        private record TokenResponse(string Token, string RefreshToken);
        private record ProductResponse(Guid Id, string Name, string Url);
    }
}
