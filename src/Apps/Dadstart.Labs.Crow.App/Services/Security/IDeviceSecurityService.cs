namespace Dadstart.Labs.Crow.Services.Security;

public interface IDeviceSecurityService
{
    Task<bool> IsBiometricAvailableAsync(CancellationToken cancellationToken = default);

    Task<bool> AuthenticateWithBiometricsAsync(string reason, CancellationToken cancellationToken = default);

    Task<string> GetOrCreateDeviceIdAsync();

    Task<string?> GetPersistedBiometricTokenAsync();

    Task PersistBiometricTokenAsync(string token);
}

