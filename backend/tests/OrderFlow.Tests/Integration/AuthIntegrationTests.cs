using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace OrderFlow.Tests.Integration;

public class AuthIntegrationTests : IClassFixture<OrderFlowWebApplicationFactory>
{
    private readonly HttpClient _client;
    private static readonly JsonSerializerOptions Json = new() { PropertyNameCaseInsensitive = true };

    public AuthIntegrationTests(OrderFlowWebApplicationFactory factory)
        => _client = factory.CreateClient();

    // ── POST /api/v1/auth/register ─────────────────────────────────────────

    [Fact]
    public async Task Register_WithValidPayload_Returns201()
    {
        var payload = new { username = "testuser1", email = "t1@orderflow.test",
                            password = "SecurePass1!", fullName = "Test User" };

        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", payload);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Register_DuplicateUsername_Returns400()
    {
        var payload = new { username = "dupuser", email = "dup1@test.com",
                            password = "SecurePass1!", fullName = "Dup" };
        await _client.PostAsJsonAsync("/api/v1/auth/register", payload);

        var dup = new { username = "dupuser", email = "dup2@test.com",
                        password = "SecurePass1!", fullName = "Dup2" };
        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", dup);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // ── POST /api/v1/auth/login ────────────────────────────────────────────

    [Fact]
    public async Task Login_WithValidCredentials_Returns200AndToken()
    {
        var reg = new { username = "loginuser", email = "login@orderflow.test",
                        password = "SecurePass1!", fullName = "Login" };
        await _client.PostAsJsonAsync("/api/v1/auth/register", reg);

        var login    = new { username = "loginuser", password = "SecurePass1!" };
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", login);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(Json);
        Assert.True(body.TryGetProperty("token", out var token));
        Assert.NotEmpty(token.GetString()!);
    }

    [Fact]
    public async Task Login_WrongPassword_Returns401()
    {
        var login    = new { username = "nobody", password = "WrongPass1!" };
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", login);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // ── Protected endpoints ────────────────────────────────────────────────

    [Fact]
    public async Task GetOrders_WithoutToken_Returns401()
        => Assert.Equal(HttpStatusCode.Unauthorized,
            (await _client.GetAsync("/api/v1/orders")).StatusCode);

    [Fact]
    public async Task GetProducts_WithoutToken_Returns200()
        => Assert.Equal(HttpStatusCode.OK,
            (await _client.GetAsync("/api/v1/products")).StatusCode);

    [Fact]
    public async Task HealthEndpoint_Returns200()
        => Assert.Equal(HttpStatusCode.OK,
            (await _client.GetAsync("/health")).StatusCode);
}
