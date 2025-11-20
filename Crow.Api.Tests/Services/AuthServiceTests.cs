using Dadstart.Labs.Crow.Api.Services;
using Dadstart.Labs.Crow.Models;
using Dadstart.Labs.Crow.Models.Dtos;
using Dadstart.Labs.Crow.Models.Factories;
using Microsoft.Extensions.Configuration;
using Moq;

namespace Dadstart.Labs.Crow.Api.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<IStorageService> _mockStorage;
    private readonly Mock<IPasswordHasher> _mockPasswordHasher;
    private readonly Mock<IJwtTokenService> _mockJwtTokenService;
    private readonly UserFactory _userFactory;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _mockStorage = new Mock<IStorageService>();
        _mockPasswordHasher = new Mock<IPasswordHasher>();
        _mockJwtTokenService = new Mock<IJwtTokenService>();
        _userFactory = new UserFactory(TimeProvider.System);
        _authService = new AuthService(_mockStorage.Object, _mockPasswordHasher.Object, _mockJwtTokenService.Object, _userFactory);
    }

    [Fact]
    public async Task RegisterAsync_ShouldCreateUser_WhenValid()
    {
        var dto = new RegisterDto("testuser", "test@example.com", "password123");
        var passwordHash = "hashedPassword";
        var user = _userFactory.Create(dto.Username, dto.Email, passwordHash);
        var token = "jwt_token";
        var refreshToken = "refresh_token";

        _mockStorage.Setup(s => s.GetUserByUsernameAsync(dto.Username)).ReturnsAsync((User?)null);
        _mockStorage.Setup(s => s.GetUserByEmailAsync(dto.Email)).ReturnsAsync((User?)null);
        _mockPasswordHasher.Setup(h => h.HashPassword(dto.Password)).Returns(passwordHash);
        _mockStorage.Setup(s => s.CreateUserAsync(It.IsAny<User>())).ReturnsAsync(user);
        _mockJwtTokenService.Setup(j => j.GenerateToken(user.Id, user.Username)).Returns(token);
        _mockJwtTokenService.Setup(j => j.GenerateRefreshToken()).Returns(refreshToken);
        _mockStorage.Setup(s => s.StoreRefreshTokenAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<DateTime>())).Returns(Task.CompletedTask);

        var result = await _authService.RegisterAsync(dto);

        Assert.NotNull(result);
        Assert.Equal(token, result.Token);
        Assert.Equal(refreshToken, result.RefreshToken);
        Assert.Equal(user.Username, result.Username);
        Assert.Equal(user.Id, result.UserId);
        _mockStorage.Verify(s => s.StoreRefreshTokenAsync(user.Id, refreshToken, It.IsAny<DateTime>()), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_ShouldReturnNull_WhenUsernameExists()
    {
        var dto = new RegisterDto("testuser", "test@example.com", "password123");
        var existingUser = _userFactory.Create("testuser", "other@example.com", "hash");

        _mockStorage.Setup(s => s.GetUserByUsernameAsync(dto.Username)).ReturnsAsync(existingUser);

        var result = await _authService.RegisterAsync(dto);

        Assert.Null(result);
        _mockStorage.Verify(s => s.CreateUserAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task RegisterAsync_ShouldReturnNull_WhenEmailExists()
    {
        var dto = new RegisterDto("testuser", "test@example.com", "password123");
        var existingUser = _userFactory.Create("otheruser", "test@example.com", "hash");

        _mockStorage.Setup(s => s.GetUserByUsernameAsync(dto.Username)).ReturnsAsync((User?)null);
        _mockStorage.Setup(s => s.GetUserByEmailAsync(dto.Email)).ReturnsAsync(existingUser);

        var result = await _authService.RegisterAsync(dto);

        Assert.Null(result);
        _mockStorage.Verify(s => s.CreateUserAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnToken_WhenValidCredentials()
    {
        var dto = new LoginDto("testuser", "password123");
        var passwordHash = "hashedPassword";
        var user = _userFactory.Create(dto.Username, "test@example.com", passwordHash);
        var token = "jwt_token";
        var refreshToken = "refresh_token";

        _mockStorage.Setup(s => s.GetUserByUsernameAsync(dto.Username)).ReturnsAsync(user);
        _mockPasswordHasher.Setup(h => h.VerifyPassword(dto.Password, passwordHash)).Returns(true);
        _mockJwtTokenService.Setup(j => j.GenerateToken(user.Id, user.Username)).Returns(token);
        _mockJwtTokenService.Setup(j => j.GenerateRefreshToken()).Returns(refreshToken);
        _mockStorage.Setup(s => s.StoreRefreshTokenAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<DateTime>())).Returns(Task.CompletedTask);

        var result = await _authService.LoginAsync(dto);

        Assert.NotNull(result);
        Assert.Equal(token, result.Token);
        Assert.Equal(refreshToken, result.RefreshToken);
        Assert.Equal(user.Username, result.Username);
        _mockStorage.Verify(s => s.StoreRefreshTokenAsync(user.Id, refreshToken, It.IsAny<DateTime>()), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnNull_WhenUserNotFound()
    {
        var dto = new LoginDto("testuser", "password123");

        _mockStorage.Setup(s => s.GetUserByUsernameAsync(dto.Username)).ReturnsAsync((User?)null);

        var result = await _authService.LoginAsync(dto);

        Assert.Null(result);
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnNull_WhenInvalidPassword()
    {
        var dto = new LoginDto("testuser", "wrongpassword");
        var passwordHash = "hashedPassword";
        var user = _userFactory.Create(dto.Username, "test@example.com", passwordHash);

        _mockStorage.Setup(s => s.GetUserByUsernameAsync(dto.Username)).ReturnsAsync(user);
        _mockPasswordHasher.Setup(h => h.VerifyPassword(dto.Password, passwordHash)).Returns(false);

        var result = await _authService.LoginAsync(dto);

        Assert.Null(result);
        _mockJwtTokenService.Verify(j => j.GenerateToken(It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
        _mockJwtTokenService.Verify(j => j.GenerateRefreshToken(), Times.Never);
    }

    [Fact]
    public async Task RefreshTokenAsync_ShouldReturnNewTokens_WhenValid()
    {
        var refreshToken = "valid_refresh_token";
        var userId = Guid.NewGuid();
        var user = _userFactory.Create("testuser", "test@example.com", "hash");
        var newToken = "new_jwt_token";
        var newRefreshToken = "new_refresh_token";

        _mockStorage.Setup(s => s.GetUserIdByRefreshTokenAsync(refreshToken)).ReturnsAsync(userId);
        _mockStorage.Setup(s => s.GetUserByIdAsync(userId)).ReturnsAsync(user);
        _mockStorage.Setup(s => s.RevokeRefreshTokenAsync(refreshToken)).Returns(Task.CompletedTask);
        _mockJwtTokenService.Setup(j => j.GenerateToken(user.Id, user.Username)).Returns(newToken);
        _mockJwtTokenService.Setup(j => j.GenerateRefreshToken()).Returns(newRefreshToken);
        _mockStorage.Setup(s => s.StoreRefreshTokenAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<DateTime>())).Returns(Task.CompletedTask);

        var result = await _authService.RefreshTokenAsync(refreshToken);

        Assert.NotNull(result);
        Assert.Equal(newToken, result.Token);
        Assert.Equal(newRefreshToken, result.RefreshToken);
        Assert.Equal(user.Username, result.Username);
        Assert.Equal(user.Id, result.UserId);
        _mockStorage.Verify(s => s.RevokeRefreshTokenAsync(refreshToken), Times.Once);
        _mockStorage.Verify(s => s.StoreRefreshTokenAsync(user.Id, newRefreshToken, It.IsAny<DateTime>()), Times.Once);
    }

    [Fact]
    public async Task RefreshTokenAsync_ShouldReturnNull_WhenInvalidToken()
    {
        var refreshToken = "invalid_refresh_token";

        _mockStorage.Setup(s => s.GetUserIdByRefreshTokenAsync(refreshToken)).ReturnsAsync((Guid?)null);

        var result = await _authService.RefreshTokenAsync(refreshToken);

        Assert.Null(result);
        _mockStorage.Verify(s => s.RevokeRefreshTokenAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task RefreshTokenAsync_ShouldReturnNull_WhenUserNotFound()
    {
        var refreshToken = "valid_refresh_token";
        var userId = Guid.NewGuid();

        _mockStorage.Setup(s => s.GetUserIdByRefreshTokenAsync(refreshToken)).ReturnsAsync(userId);
        _mockStorage.Setup(s => s.GetUserByIdAsync(userId)).ReturnsAsync((User?)null);

        var result = await _authService.RefreshTokenAsync(refreshToken);

        Assert.Null(result);
        _mockStorage.Verify(s => s.RevokeRefreshTokenAsync(It.IsAny<string>()), Times.Never);
    }
}

