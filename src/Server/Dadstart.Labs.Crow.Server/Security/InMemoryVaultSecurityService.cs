namespace Dadstart.Labs.Crow.Server.Security;

using Microsoft.Extensions.Logging;

internal sealed class InMemoryVaultSecurityService(
    VaultSecretsStore store,
    IVaultSessionService sessionService,
    ILogger<InMemoryVaultSecurityService> logger) : IVaultSecurityService
{
    const int SessionLifetimeMinutes = 45;

    public Task<VaultSetupState> ConfigureAsync(VaultSetupRequest request, CancellationToken cancellationToken)
    {
        var state = store.Configure(request);
        logger.LogInformation("Vault configured. Biometric preferred: {Preferred}", state.BiometricPreferred);
        return Task.FromResult(state);
    }

    public Task<VaultSetupState> GetSetupStateAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult(store.GetState());
    }

    public async Task RegisterBiometricAsync(string sessionToken, string deviceId, string biometricToken, CancellationToken cancellationToken)
    {
        var session = await sessionService.GetAsync(sessionToken, cancellationToken);
        if (session is null)
        {
            throw new InvalidOperationException("Session expired.");
        }

        store.RegisterBiometric(deviceId, biometricToken);
        logger.LogInformation("Registered biometric token for device {Device}", deviceId);
    }

    public async Task<UnlockResponse> UnlockAsync(UnlockRequest request, CancellationToken cancellationToken)
    {
        var (isValid, deviceId) = request.Method switch
        {
            AuthenticationMethod.Pin => (store.ValidatePin(request.Pin), request.DeviceId ?? "pin-device"),
            AuthenticationMethod.MasterPassword => (store.ValidatePassword(request.MasterPassword), request.DeviceId ?? "password-device"),
            AuthenticationMethod.Biometric => (store.ValidateBiometric(request.DeviceId, request.BiometricToken), request.DeviceId ?? "biometric-device"),
            _ => (false, request.DeviceId ?? "unknown")
        };

        if (!isValid)
        {
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        var session = await sessionService.IssueAsync(deviceId, TimeSpan.FromMinutes(SessionLifetimeMinutes), cancellationToken);
        return new UnlockResponse
        {
            SessionToken = session.Token,
            ExpiresAt = session.ExpiresAt,
            BiometricAllowed = store.GetState().BiometricPreferred
        };
    }
}

