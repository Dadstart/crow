namespace Dadstart.Labs.Crow.Security;

public sealed record class BiometricRegistrationRequest
{
    public required string DeviceId { get; init; }

    public required string BiometricToken { get; init; }
}

