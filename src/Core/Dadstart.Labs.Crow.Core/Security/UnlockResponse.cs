namespace Dadstart.Labs.Crow.Security;

public sealed record class UnlockResponse
{
    public required string SessionToken { get; init; }

    public DateTimeOffset ExpiresAt { get; init; }

    public bool BiometricAllowed { get; init; }
}

