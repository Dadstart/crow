using Dadstart.Labs.Crow.Api.Controllers;
using Dadstart.Labs.Crow.Api.Services;
using Dadstart.Labs.Crow.Models;
using Dadstart.Labs.Crow.Models.Dtos;
using Dadstart.Labs.Crow.Models.Factories;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Dadstart.Labs.Crow.Api.Tests.Controllers;

public class PasswordsControllerEncryptionTests
{
    private readonly Mock<IStorageService> _mockStorage;
    private readonly Mock<IEncryptionService> _mockEncryption;
    private readonly PasswordFactory _passwordFactory;
    private readonly PasswordsController _controller;

    public PasswordsControllerEncryptionTests()
    {
        _mockStorage = new Mock<IStorageService>();
        _mockEncryption = new Mock<IEncryptionService>();
        _passwordFactory = new PasswordFactory(TimeProvider.System);
        _controller = new PasswordsController(_mockStorage.Object, _mockEncryption.Object, _passwordFactory);
    }

    [Fact]
    public async Task Create_ShouldEncryptPassword_BeforeStoring()
    {
        var dto = new CreatePasswordDto("Test", "user", "plainPassword123", null, null);
        var encryptedPassword = "encryptedPassword123";
        var storedPassword = _passwordFactory.Create("Test", "user", encryptedPassword, null, null);

        _mockEncryption.Setup(e => e.Encrypt("plainPassword123")).Returns(encryptedPassword);
        _mockStorage.Setup(s => s.CreatePasswordAsync(It.IsAny<Password>())).ReturnsAsync(storedPassword);

        var result = await _controller.Create(dto);

        _mockEncryption.Verify(e => e.Encrypt("plainPassword123"), Times.Once);
        _mockStorage.Verify(s => s.CreatePasswordAsync(It.Is<Password>(p => p.EncryptedPassword == encryptedPassword)), Times.Once);
        
        var createdAtResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var response = Assert.IsType<PasswordResponseDto>(createdAtResult.Value);
        Assert.Equal("plainPassword123", response.DecryptedPassword);
    }

    [Fact]
    public async Task GetAll_ShouldDecryptPasswords()
    {
        var encryptedPassword = "encrypted123";
        var decryptedPassword = "plainPassword123";
        var password = _passwordFactory.Create("Test", "user", encryptedPassword, null, null);
        var passwords = new List<Password> { password };

        _mockStorage.Setup(s => s.GetAllPasswordsAsync()).ReturnsAsync(passwords);
        _mockEncryption.Setup(e => e.Decrypt(encryptedPassword)).Returns(decryptedPassword);

        var result = await _controller.GetAll();

        _mockEncryption.Verify(e => e.Decrypt(encryptedPassword), Times.Once);
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsAssignableFrom<List<PasswordResponseDto>>(okResult.Value);
        Assert.Equal(decryptedPassword, response[0].DecryptedPassword);
    }

    [Fact]
    public async Task Update_ShouldEncryptPassword_WhenProvided()
    {
        var existing = _passwordFactory.Create("Original", "user", "oldEncrypted", null, null);
        var dto = new UpdatePasswordDto("Updated", null, "newPlainPassword", null, null);
        var newEncrypted = "newEncryptedPassword";
        var updated = _passwordFactory.WithUpdate(existing, "Updated", null, newEncrypted, null, null);

        _mockStorage.Setup(s => s.GetPasswordByIdAsync(existing.Id)).ReturnsAsync(existing);
        _mockEncryption.Setup(e => e.Encrypt("newPlainPassword")).Returns(newEncrypted);
        _mockStorage.Setup(s => s.UpdatePasswordAsync(existing.Id, It.IsAny<Password>())).ReturnsAsync(updated);

        var result = await _controller.Update(existing.Id, dto);

        _mockEncryption.Verify(e => e.Encrypt("newPlainPassword"), Times.Once);
        _mockStorage.Verify(s => s.UpdatePasswordAsync(existing.Id, It.Is<Password>(p => p.EncryptedPassword == newEncrypted)), Times.Once);
    }

    [Fact]
    public async Task Update_ShouldNotEncrypt_WhenPasswordNotProvided()
    {
        var existing = _passwordFactory.Create("Original", "user", "existingEncrypted", null, null);
        var dto = new UpdatePasswordDto("Updated", null, null, null, null);
        var updated = _passwordFactory.WithUpdate(existing, "Updated", null, null, null, null);

        _mockStorage.Setup(s => s.GetPasswordByIdAsync(existing.Id)).ReturnsAsync(existing);
        _mockStorage.Setup(s => s.UpdatePasswordAsync(existing.Id, It.IsAny<Password>())).ReturnsAsync(updated);
        _mockEncryption.Setup(e => e.Decrypt("existingEncrypted")).Returns("decrypted");

        var result = await _controller.Update(existing.Id, dto);

        _mockEncryption.Verify(e => e.Encrypt(It.IsAny<string>()), Times.Never);
        _mockStorage.Verify(s => s.UpdatePasswordAsync(existing.Id, It.Is<Password>(p => p.EncryptedPassword == "existingEncrypted")), Times.Once);
    }
}

