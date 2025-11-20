using System.Security.Cryptography;
using System.Text;

namespace Dadstart.Labs.Crow.Api.Services;

public class AesEncryptionService : IEncryptionService
{
    private readonly byte[] _key;
    private const int KeySize = 32;
    private const int IvSize = 12;
    private const int TagSize = 16;

    public AesEncryptionService(IConfiguration configuration)
    {
        var masterKey = configuration["Encryption:MasterKey"];
        if (string.IsNullOrWhiteSpace(masterKey))
        {
            throw new InvalidOperationException("Encryption:MasterKey must be configured in appsettings.json");
        }

        _key = DeriveKey(masterKey);
    }

    private static byte[] DeriveKey(string masterPassword)
    {
        var salt = Encoding.UTF8.GetBytes("CrowPasswordManagerSalt");
        using var pbkdf2 = new Rfc2898DeriveBytes(masterPassword, salt, 100000, HashAlgorithmName.SHA256);
        return pbkdf2.GetBytes(KeySize);
    }

    public string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            return string.Empty;

        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var iv = new byte[IvSize];
        RandomNumberGenerator.Fill(iv);

        var ciphertext = new byte[plainBytes.Length];
        var tag = new byte[TagSize];

        using var aesGcm = new AesGcm(_key, TagSize);
        aesGcm.Encrypt(iv, plainBytes, ciphertext, tag);

        var result = new byte[IvSize + ciphertext.Length + TagSize];
        Buffer.BlockCopy(iv, 0, result, 0, IvSize);
        Buffer.BlockCopy(ciphertext, 0, result, IvSize, ciphertext.Length);
        Buffer.BlockCopy(tag, 0, result, IvSize + ciphertext.Length, TagSize);

        return Convert.ToBase64String(result);
    }

    public string Decrypt(string encryptedText)
    {
        if (string.IsNullOrEmpty(encryptedText))
            return string.Empty;

        try
        {
            var encryptedBytes = Convert.FromBase64String(encryptedText);
            if (encryptedBytes.Length < IvSize + TagSize)
                throw new CryptographicException("Invalid encrypted data length");

            var iv = new byte[IvSize];
            var tag = new byte[TagSize];
            var ciphertext = new byte[encryptedBytes.Length - IvSize - TagSize];

            Buffer.BlockCopy(encryptedBytes, 0, iv, 0, IvSize);
            Buffer.BlockCopy(encryptedBytes, IvSize, ciphertext, 0, ciphertext.Length);
            Buffer.BlockCopy(encryptedBytes, IvSize + ciphertext.Length, tag, 0, TagSize);

            var plaintext = new byte[ciphertext.Length];
            using var aesGcm = new AesGcm(_key, TagSize);
            aesGcm.Decrypt(iv, ciphertext, tag, plaintext);

            return Encoding.UTF8.GetString(plaintext);
        }
        catch (Exception ex) when (ex is CryptographicException or FormatException)
        {
            throw new CryptographicException("Failed to decrypt password", ex);
        }
    }
}

