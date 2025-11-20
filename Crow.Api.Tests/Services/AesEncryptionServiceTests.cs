using Dadstart.Labs.Crow.Api.Services;
using Microsoft.Extensions.Configuration;
using Moq;

namespace Dadstart.Labs.Crow.Api.Tests.Services;

public class AesEncryptionServiceTests
{
    private readonly IEncryptionService _encryptionService;

    public AesEncryptionServiceTests()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Encryption:MasterKey", "TestMasterKey123" }
            })
            .Build();

        _encryptionService = new AesEncryptionService(configuration);
    }

    [Fact]
    public void Encrypt_ShouldReturnEncryptedString()
    {
        var plainText = "MySecretPassword123";

        var encrypted = _encryptionService.Encrypt(plainText);

        Assert.NotEmpty(encrypted);
        Assert.NotEqual(plainText, encrypted);
        Assert.DoesNotContain(plainText, encrypted);
    }

    [Fact]
    public void Encrypt_ShouldReturnDifferentResults_ForSameInput()
    {
        var plainText = "SamePassword";
        var encrypted1 = _encryptionService.Encrypt(plainText);
        var encrypted2 = _encryptionService.Encrypt(plainText);

        Assert.NotEqual(encrypted1, encrypted2);
    }

    [Fact]
    public void Decrypt_ShouldReturnOriginalPlainText()
    {
        var plainText = "MySecretPassword123";
        var encrypted = _encryptionService.Encrypt(plainText);

        var decrypted = _encryptionService.Decrypt(encrypted);

        Assert.Equal(plainText, decrypted);
    }

    [Fact]
    public void EncryptDecrypt_ShouldWorkWithVariousInputs()
    {
        var testCases = new[]
        {
            "SimplePassword",
            "Complex!P@ssw0rd#123",
            "VeryLongPasswordThatContainsManyCharactersAndSymbols!@#$%^&*()",
            "1234567890",
            "SpecialChars: !@#$%^&*()_+-=[]{}|;':\",./<>?",
            string.Empty
        };

        foreach (var testCase in testCases)
        {
            var encrypted = _encryptionService.Encrypt(testCase);
            var decrypted = _encryptionService.Decrypt(encrypted);
            Assert.Equal(testCase, decrypted);
        }
    }

    [Fact]
    public void Encrypt_ShouldReturnEmptyString_WhenInputIsEmpty()
    {
        var encrypted = _encryptionService.Encrypt(string.Empty);

        Assert.Equal(string.Empty, encrypted);
    }

    [Fact]
    public void Decrypt_ShouldReturnEmptyString_WhenInputIsEmpty()
    {
        var decrypted = _encryptionService.Decrypt(string.Empty);

        Assert.Equal(string.Empty, decrypted);
    }

    [Fact]
    public void Decrypt_ShouldThrow_WhenInvalidEncryptedData()
    {
        var invalidEncrypted = "NotAValidBase64String!!!";

        Assert.Throws<System.Security.Cryptography.CryptographicException>(() => 
            _encryptionService.Decrypt(invalidEncrypted));
    }

    [Fact]
    public void Decrypt_ShouldThrow_WhenTamperedData()
    {
        var plainText = "MyPassword";
        var encrypted = _encryptionService.Encrypt(plainText);
        var tampered = encrypted.Substring(0, encrypted.Length - 5) + "XXXXX";

        Assert.Throws<System.Security.Cryptography.CryptographicException>(() => 
            _encryptionService.Decrypt(tampered));
    }
}

