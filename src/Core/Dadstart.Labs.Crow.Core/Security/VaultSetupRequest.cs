namespace Dadstart.Labs.Crow.Security;

public sealed record class VaultSetupRequest
{
    public required string Pin { get; init; }

    public string? MasterPassword { get; init; }

    public bool EnableBiometric { get; init; }
}

