namespace Dadstart.Labs.Crow.Security;

public sealed record class BiometricProfile
{
    public required string DeviceId { get; init; }

    public required string Token { get; init; }

    public DateTimeOffset RegisteredAt { get; init; } = DateTimeOffset.UtcNow;
}

