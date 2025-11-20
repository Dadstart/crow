using Dadstart.Labs.Crow.Models;
using Dadstart.Labs.Crow.Models.Factories;

namespace Dadstart.Labs.Crow.Models.Tests;

public class PasswordTests
{
    private readonly PasswordFactory _factory = new(TimeProvider.System);

    [Fact]
    public void Create_ShouldSetAllProperties()
    {
        var password = _factory.Create("Test", "username", "encrypted123", "https://example.com", "Notes");

        Assert.NotEqual(Guid.Empty, password.Id);
        Assert.Equal("Test", password.Title);
        Assert.Equal("username", password.Username);
        Assert.Equal("encrypted123", password.EncryptedPassword);
        Assert.Equal("https://example.com", password.Url);
        Assert.Equal("Notes", password.Notes);
        Assert.True(password.CreatedAt <= TimeProvider.System.GetUtcNow());
    }

    [Fact]
    public void Create_ShouldAllowNullOptionalFields()
    {
        var password = _factory.Create("Test", "user", "enc", null, null);

        Assert.Null(password.Url);
        Assert.Null(password.Notes);
    }

    [Fact]
    public void WithUpdate_ShouldUpdateSpecifiedFields()
    {
        var original = _factory.Create("Original", "user", "enc", "url", "notes");
        var updated = _factory.WithUpdate(original, "Updated", "newuser", "newenc", "newurl", "newnotes");

        Assert.Equal("Updated", updated.Title);
        Assert.Equal("newuser", updated.Username);
        Assert.Equal("newenc", updated.EncryptedPassword);
        Assert.Equal("newurl", updated.Url);
        Assert.Equal("newnotes", updated.Notes);
        Assert.True(updated.UpdatedAt > original.UpdatedAt);
    }

    [Fact]
    public void WithUpdate_ShouldPreserveOriginalValues_WhenNull()
    {
        var original = _factory.Create("Original", "user", "enc", "url", "notes");
        var updated = _factory.WithUpdate(original, null, null, null, null, null);

        Assert.Equal("Original", updated.Title);
        Assert.Equal("user", updated.Username);
        Assert.Equal("enc", updated.EncryptedPassword);
        Assert.True(updated.UpdatedAt > original.UpdatedAt);
    }
}

