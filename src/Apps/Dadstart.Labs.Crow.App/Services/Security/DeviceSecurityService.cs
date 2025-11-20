namespace Dadstart.Labs.Crow.Services.Security;

using Microsoft.Maui.Storage;

public sealed partial class DeviceSecurityService : IDeviceSecurityService
{
    const string DeviceIdKey = "crow_device_id";
    const string BiometricTokenKey = "crow_biometric_token";

    public Task<bool> AuthenticateWithBiometricsAsync(string reason, CancellationToken cancellationToken = default)
        => PlatformAuthenticateAsync(reason, cancellationToken);

    public Task<string> GetOrCreateDeviceIdAsync()
    {
        var stored = Preferences.Default.Get(DeviceIdKey, string.Empty);
        if (!string.IsNullOrWhiteSpace(stored))
        {
            return Task.FromResult(stored);
        }

        var deviceId = Guid.NewGuid().ToString("N");
        Preferences.Default.Set(DeviceIdKey, deviceId);
        return Task.FromResult(deviceId);
    }

    public async Task<string?> GetPersistedBiometricTokenAsync()
    {
        try
        {
            return await SecureStorage.Default.GetAsync(BiometricTokenKey);
        }
        catch
        {
            return null;
        }
    }

    public Task PersistBiometricTokenAsync(string token)
        => SecureStorage.Default.SetAsync(BiometricTokenKey, token);

    public Task<bool> IsBiometricAvailableAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(PlatformIsBiometricAvailable());

    private partial bool PlatformIsBiometricAvailable();

    private partial Task<bool> PlatformAuthenticateAsync(string reason, CancellationToken cancellationToken);
}

