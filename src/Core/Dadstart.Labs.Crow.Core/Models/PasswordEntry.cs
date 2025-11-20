namespace Dadstart.Labs.Crow.Models;

/// <summary>
/// Login credential stored inside the vault.
/// </summary>
public sealed record class PasswordEntry : VaultItem
{
    public string Username { get; init; } = string.Empty;

    public string Secret { get; init; } = string.Empty;

    public Uri? ResourceUri { get; init; }

    public string Notes { get; init; } = string.Empty;

    public PasswordStrength Strength { get; init; } = PasswordStrength.Unknown;

    public DateTimeOffset? LastRotatedAt { get; init; }

    public DateTimeOffset? ExpiresAt { get; init; }
}

