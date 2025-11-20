using Dadstart.Labs.Crow.Api.Controllers;
using Dadstart.Labs.Crow.Api.Services;
using Dadstart.Labs.Crow.Models;
using Dadstart.Labs.Crow.Models.Dtos;
using Dadstart.Labs.Crow.Models.Factories;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Dadstart.Labs.Crow.Api.Tests.Controllers;

public class PasswordsControllerTests
{
    private readonly Mock<IStorageService> _mockStorage;
    private readonly Mock<IEncryptionService> _mockEncryption;
    private readonly PasswordFactory _passwordFactory;
    private readonly PasswordsController _controller;

    public PasswordsControllerTests()
    {
        _mockStorage = new Mock<IStorageService>();
        _mockEncryption = new Mock<IEncryptionService>();
        _passwordFactory = new PasswordFactory(TimeProvider.System);
        _controller = new PasswordsController(_mockStorage.Object, _mockEncryption.Object, _passwordFactory);
        
        _mockEncryption.Setup(e => e.Encrypt(It.IsAny<string>())).Returns<string>(s => $"encrypted_{s}");
        _mockEncryption.Setup(e => e.Decrypt(It.IsAny<string>())).Returns<string>(s => s.Replace("encrypted_", ""));
    }

    [Fact]
    public async Task GetAll_ShouldReturnAllPasswords()
    {
        var passwords = new List<Password>
        {
            _passwordFactory.Create("Pwd 1", "user1", "enc1"),
            _passwordFactory.Create("Pwd 2", "user2", "enc2")
        };
        _mockStorage.Setup(s => s.GetAllPasswordsAsync()).ReturnsAsync(passwords);

        var result = await _controller.GetAll();

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedPasswords = Assert.IsAssignableFrom<List<PasswordResponseDto>>(okResult.Value);
        Assert.Equal(2, returnedPasswords.Count);
    }

    [Fact]
    public async Task GetById_ShouldReturnPassword_WhenExists()
    {
        var password = _passwordFactory.Create("Test", "user", "enc");
        _mockStorage.Setup(s => s.GetPasswordByIdAsync(password.Id)).ReturnsAsync(password);

        var result = await _controller.GetById(password.Id);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedPassword = Assert.IsType<PasswordResponseDto>(okResult.Value);
        Assert.Equal(password.Id, returnedPassword.Id);
    }

    [Fact]
    public async Task Create_ShouldCreatePassword()
    {
        var dto = new CreatePasswordDto("Test", "user", "plainPassword", "https://example.com", "Notes");
        var encryptedPassword = "encrypted_plainPassword";
        var password = _passwordFactory.Create(dto.Title, dto.Username, encryptedPassword, dto.Url, dto.Notes);
        
        _mockStorage.Setup(s => s.CreatePasswordAsync(It.IsAny<Password>())).ReturnsAsync(password);

        var result = await _controller.Create(dto);

        var createdAtResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var response = Assert.IsType<PasswordResponseDto>(createdAtResult.Value);
        Assert.Equal("Test", response.Title);
        Assert.Equal("plainPassword", response.DecryptedPassword);
    }

    [Fact]
    public async Task Update_ShouldUpdatePassword_WhenExists()
    {
        var password = _passwordFactory.Create("Original", "user", "encrypted_old");
        var dto = new UpdatePasswordDto("Updated", "newuser", "newplain", null, null);
        var updated = _passwordFactory.WithUpdate(password, dto.Title, dto.Username, "encrypted_newplain", dto.Url, dto.Notes);
        
        _mockStorage.Setup(s => s.GetPasswordByIdAsync(password.Id)).ReturnsAsync(password);
        _mockStorage.Setup(s => s.UpdatePasswordAsync(password.Id, It.IsAny<Password>())).ReturnsAsync(updated);

        var result = await _controller.Update(password.Id, dto);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedPassword = Assert.IsType<PasswordResponseDto>(okResult.Value);
        Assert.Equal("Updated", returnedPassword.Title);
    }

    [Fact]
    public async Task Delete_ShouldDeletePassword_WhenExists()
    {
        var id = Guid.NewGuid();
        _mockStorage.Setup(s => s.DeletePasswordAsync(id)).ReturnsAsync(true);

        var result = await _controller.Delete(id);

        Assert.IsType<NoContentResult>(result);
    }
}

