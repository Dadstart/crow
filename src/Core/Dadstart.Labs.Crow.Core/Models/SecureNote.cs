namespace Dadstart.Labs.Crow.Models;

/// <summary>
/// Free-form encrypted note stored inside the vault.
/// </summary>
public sealed record class SecureNote : VaultItem
{
    public string RichTextBody { get; init; } = string.Empty;
}

