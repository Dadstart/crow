namespace Dadstart.Labs.Crow.Security;

/// <summary>
/// Payload used by clients to unlock the vault.
/// </summary>
public sealed record class UnlockRequest
{
    public AuthenticationMethod Method { get; init; }

    public string? Pin { get; init; }

    public string? MasterPassword { get; init; }

    public string? BiometricToken { get; init; }

    public string? DeviceId { get; init; }
}

