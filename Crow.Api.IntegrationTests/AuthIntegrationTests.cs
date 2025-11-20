using System.Net;
using System.Net.Http.Json;
using Dadstart.Labs.Crow.Models.Dtos;
using Xunit;

namespace Dadstart.Labs.Crow.Api.IntegrationTests;

public class AuthIntegrationTests : IClassFixture<CrowWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthIntegrationTests(CrowWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_ShouldReturnToken_WhenValid()
    {
        var dto = new RegisterDto($"testuser_{Guid.NewGuid()}", $"test{Guid.NewGuid()}@example.com", "password123");

        var response = await _client.PostAsJsonAsync("/api/auth/register", dto);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        Assert.NotNull(authResponse);
        Assert.NotEmpty(authResponse.Token);
        Assert.NotEmpty(authResponse.RefreshToken);
        Assert.Equal(dto.Username, authResponse.Username);
        Assert.NotEqual(Guid.Empty, authResponse.UserId);
    }

    [Fact]
    public async Task Register_ShouldReturnBadRequest_WhenUsernameExists()
    {
        var username = $"testuser_{Guid.NewGuid()}";
        var dto1 = new RegisterDto(username, $"test1{Guid.NewGuid()}@example.com", "password123");
        var dto2 = new RegisterDto(username, $"test2{Guid.NewGuid()}@example.com", "password123");

        await _client.PostAsJsonAsync("/api/auth/register", dto1);
        var response = await _client.PostAsJsonAsync("/api/auth/register", dto2);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Register_ShouldReturnBadRequest_WhenEmailExists()
    {
        var email = $"test{Guid.NewGuid()}@example.com";
        var dto1 = new RegisterDto($"user1_{Guid.NewGuid()}", email, "password123");
        var dto2 = new RegisterDto($"user2_{Guid.NewGuid()}", email, "password123");

        await _client.PostAsJsonAsync("/api/auth/register", dto1);
        var response = await _client.PostAsJsonAsync("/api/auth/register", dto2);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Login_ShouldReturnToken_WhenValidCredentials()
    {
        var username = $"testuser_{Guid.NewGuid()}";
        var password = "password123";
        var registerDto = new RegisterDto(username, $"test{Guid.NewGuid()}@example.com", password);
        
        await _client.PostAsJsonAsync("/api/auth/register", registerDto);

        var loginDto = new LoginDto(username, password);
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginDto);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        Assert.NotNull(authResponse);
        Assert.NotEmpty(authResponse.Token);
        Assert.NotEmpty(authResponse.RefreshToken);
        Assert.Equal(username, authResponse.Username);
    }

    [Fact]
    public async Task Login_ShouldReturnUnauthorized_WhenInvalidPassword()
    {
        var username = $"testuser_{Guid.NewGuid()}";
        var registerDto = new RegisterDto(username, $"test{Guid.NewGuid()}@example.com", "password123");
        
        await _client.PostAsJsonAsync("/api/auth/register", registerDto);

        var loginDto = new LoginDto(username, "wrongpassword");
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginDto);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_ShouldReturnUnauthorized_WhenUserNotFound()
    {
        var loginDto = new LoginDto("nonexistentuser", "password123");
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginDto);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task RefreshToken_ShouldReturnNewTokens_WhenValid()
    {
        var username = $"testuser_{Guid.NewGuid()}";
        var password = "password123";
        var registerDto = new RegisterDto(username, $"test{Guid.NewGuid()}@example.com", password);
        
        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerDto);
        var authResponse = await registerResponse.Content.ReadFromJsonAsync<AuthResponseDto>();
        Assert.NotNull(authResponse);

        var refreshDto = new RefreshTokenDto(authResponse.RefreshToken);
        var response = await _client.PostAsJsonAsync("/api/auth/refresh", refreshDto);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var newAuthResponse = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        Assert.NotNull(newAuthResponse);
        Assert.NotEmpty(newAuthResponse.Token);
        Assert.NotEmpty(newAuthResponse.RefreshToken);
        Assert.NotEqual(authResponse.Token, newAuthResponse.Token);
        Assert.NotEqual(authResponse.RefreshToken, newAuthResponse.RefreshToken);
        Assert.Equal(username, newAuthResponse.Username);
    }

    [Fact]
    public async Task RefreshToken_ShouldReturnUnauthorized_WhenInvalid()
    {
        var refreshDto = new RefreshTokenDto("invalid_refresh_token");
        var response = await _client.PostAsJsonAsync("/api/auth/refresh", refreshDto);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task RefreshToken_ShouldReturnUnauthorized_WhenReused()
    {
        var username = $"testuser_{Guid.NewGuid()}";
        var password = "password123";
        var registerDto = new RegisterDto(username, $"test{Guid.NewGuid()}@example.com", password);
        
        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerDto);
        var authResponse = await registerResponse.Content.ReadFromJsonAsync<AuthResponseDto>();
        Assert.NotNull(authResponse);

        var refreshDto = new RefreshTokenDto(authResponse.RefreshToken);
        await _client.PostAsJsonAsync("/api/auth/refresh", refreshDto);

        var secondRefreshResponse = await _client.PostAsJsonAsync("/api/auth/refresh", refreshDto);

        Assert.Equal(HttpStatusCode.Unauthorized, secondRefreshResponse.StatusCode);
    }
}

