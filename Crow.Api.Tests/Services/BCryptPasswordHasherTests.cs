using Dadstart.Labs.Crow.Api.Services;

namespace Dadstart.Labs.Crow.Api.Tests.Services;

public class BCryptPasswordHasherTests
{
    private readonly IPasswordHasher _passwordHasher;

    public BCryptPasswordHasherTests()
    {
        _passwordHasher = new BCryptPasswordHasher();
    }

    [Fact]
    public void HashPassword_ShouldReturnDifferentHash_ForSamePassword()
    {
        var password = "testPassword123";
        var hash1 = _passwordHasher.HashPassword(password);
        var hash2 = _passwordHasher.HashPassword(password);

        Assert.NotEqual(hash1, hash2);
        Assert.NotEmpty(hash1);
        Assert.NotEmpty(hash2);
    }

    [Fact]
    public void VerifyPassword_ShouldReturnTrue_ForCorrectPassword()
    {
        var password = "testPassword123";
        var hash = _passwordHasher.HashPassword(password);

        var result = _passwordHasher.VerifyPassword(password, hash);

        Assert.True(result);
    }

    [Fact]
    public void VerifyPassword_ShouldReturnFalse_ForIncorrectPassword()
    {
        var password = "testPassword123";
        var wrongPassword = "wrongPassword";
        var hash = _passwordHasher.HashPassword(password);

        var result = _passwordHasher.VerifyPassword(wrongPassword, hash);

        Assert.False(result);
    }
}

