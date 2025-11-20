using Dadstart.Labs.Crow.Api.Controllers;
using Dadstart.Labs.Crow.Api.Services;
using Dadstart.Labs.Crow.Models.Dtos;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Dadstart.Labs.Crow.Api.Tests.Controllers;

public class AuthControllerTests
{
    private readonly Mock<IAuthService> _mockAuthService;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _mockAuthService = new Mock<IAuthService>();
        _controller = new AuthController(_mockAuthService.Object);
    }

    [Fact]
    public async Task Register_ShouldReturnOk_WhenSuccessful()
    {
        var dto = new RegisterDto("testuser", "test@example.com", "password123");
        var authResponse = new AuthResponseDto("token", "refreshToken", "testuser", Guid.NewGuid());

        _mockAuthService.Setup(s => s.RegisterAsync(dto)).ReturnsAsync(authResponse);

        var result = await _controller.Register(dto);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<AuthResponseDto>(okResult.Value);
        Assert.Equal("token", response.Token);
        Assert.Equal("refreshToken", response.RefreshToken);
    }

    [Fact]
    public async Task Register_ShouldReturnBadRequest_WhenUserExists()
    {
        var dto = new RegisterDto("testuser", "test@example.com", "password123");

        _mockAuthService.Setup(s => s.RegisterAsync(dto)).ReturnsAsync((AuthResponseDto?)null);

        var result = await _controller.Register(dto);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Contains("already exists", badRequestResult.Value?.ToString() ?? string.Empty);
    }

    [Fact]
    public async Task Login_ShouldReturnOk_WhenSuccessful()
    {
        var dto = new LoginDto("testuser", "password123");
        var authResponse = new AuthResponseDto("token", "refreshToken", "testuser", Guid.NewGuid());

        _mockAuthService.Setup(s => s.LoginAsync(dto)).ReturnsAsync(authResponse);

        var result = await _controller.Login(dto);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<AuthResponseDto>(okResult.Value);
        Assert.Equal("token", response.Token);
        Assert.Equal("refreshToken", response.RefreshToken);
    }

    [Fact]
    public async Task Login_ShouldReturnUnauthorized_WhenInvalidCredentials()
    {
        var dto = new LoginDto("testuser", "wrongpassword");

        _mockAuthService.Setup(s => s.LoginAsync(dto)).ReturnsAsync((AuthResponseDto?)null);

        var result = await _controller.Login(dto);

        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result.Result);
        Assert.Contains("Invalid", unauthorizedResult.Value?.ToString() ?? string.Empty);
    }

    [Fact]
    public async Task RefreshToken_ShouldReturnOk_WhenValid()
    {
        var dto = new RefreshTokenDto("refreshToken");
        var authResponse = new AuthResponseDto("newToken", "newRefreshToken", "testuser", Guid.NewGuid());

        _mockAuthService.Setup(s => s.RefreshTokenAsync(dto.RefreshToken)).ReturnsAsync(authResponse);

        var result = await _controller.RefreshToken(dto);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<AuthResponseDto>(okResult.Value);
        Assert.Equal("newToken", response.Token);
        Assert.Equal("newRefreshToken", response.RefreshToken);
    }

    [Fact]
    public async Task RefreshToken_ShouldReturnUnauthorized_WhenInvalid()
    {
        var dto = new RefreshTokenDto("invalidToken");

        _mockAuthService.Setup(s => s.RefreshTokenAsync(dto.RefreshToken)).ReturnsAsync((AuthResponseDto?)null);

        var result = await _controller.RefreshToken(dto);

        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result.Result);
        Assert.Contains("Invalid", unauthorizedResult.Value?.ToString() ?? string.Empty);
    }
}

