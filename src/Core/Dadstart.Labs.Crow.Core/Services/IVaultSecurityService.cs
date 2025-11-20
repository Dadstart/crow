namespace Dadstart.Labs.Crow.Services;

using Dadstart.Labs.Crow.Security;

public interface IVaultSecurityService
{
    Task<VaultSetupState> GetSetupStateAsync(CancellationToken cancellationToken);

    Task<VaultSetupState> ConfigureAsync(VaultSetupRequest request, CancellationToken cancellationToken);

    Task<UnlockResponse> UnlockAsync(UnlockRequest request, CancellationToken cancellationToken);

    Task RegisterBiometricAsync(string sessionToken, string deviceId, string biometricToken, CancellationToken cancellationToken);
}

