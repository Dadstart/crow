namespace Dadstart.Labs.Crow.Server.Security;

using System.Security.Cryptography;

internal static class SecretHasher
{
    const string Algorithm = "PBKDF2-SHA256";
    const int Iterations = 210_000;
    const int SaltLength = 32;
    const int HashLength = 32;

    public static MasterSecretHash HashSecret(string secret)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltLength);
        var hash = Hash(secret, salt, Iterations);
        return new MasterSecretHash
        {
            Algorithm = Algorithm,
            Salt = salt,
            Hash = hash,
            Iterations = Iterations
        };
    }

    public static bool Verify(MasterSecretHash storedHash, string providedSecret)
    {
        if (string.IsNullOrEmpty(providedSecret))
        {
            return false;
        }

        var computed = Hash(providedSecret, storedHash.Salt, storedHash.Iterations);
        return CryptographicOperations.FixedTimeEquals(storedHash.Hash, computed);
    }

    static byte[] Hash(string secret, byte[] salt, int iterations)
    {
        return Rfc2898DeriveBytes.Pbkdf2(
            secret,
            salt,
            iterations,
            HashAlgorithmName.SHA256,
            HashLength);
    }
}

