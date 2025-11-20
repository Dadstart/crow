namespace Dadstart.Labs.Crow.Security;

public sealed record class VaultSession
{
    public required string Token { get; init; }

    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;

    public DateTimeOffset ExpiresAt { get; init; } = DateTimeOffset.UtcNow.AddHours(1);

    public string? DeviceId { get; init; }
}

