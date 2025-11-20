namespace Dadstart.Labs.Crow.Server.Security;

using System.Collections.Concurrent;

internal sealed class VaultSecretsStore
{
    readonly object _gate = new();
    MasterSecretHash? _pinHash;
    MasterSecretHash? _passwordHash;
    bool _biometricPreferred;
    readonly ConcurrentDictionary<string, BiometricProfile> _biometricProfiles = new(StringComparer.OrdinalIgnoreCase);

    public VaultSetupState GetState()
    {
        lock (_gate)
        {
            return GetStateUnsafe();
        }
    }

    public VaultSetupState Configure(VaultSetupRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        lock (_gate)
        {
            if (_pinHash is not null)
            {
                throw new InvalidOperationException("Vault is already configured.");
            }

            _pinHash = SecretHasher.HashSecret(request.Pin);
            if (!string.IsNullOrWhiteSpace(request.MasterPassword))
            {
                _passwordHash = SecretHasher.HashSecret(request.MasterPassword);
            }

            _biometricPreferred = request.EnableBiometric;

            return GetStateUnsafe();
        }
    }

    public bool ValidatePin(string? pin)
    {
        if (string.IsNullOrWhiteSpace(pin))
        {
            return false;
        }

        lock (_gate)
        {
            return _pinHash is not null && SecretHasher.Verify(_pinHash, pin);
        }
    }

    public bool ValidatePassword(string? password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            return false;
        }

        lock (_gate)
        {
            return _passwordHash is not null && SecretHasher.Verify(_passwordHash, password);
        }
    }

    public bool ValidateBiometric(string? deviceId, string? token)
    {
        if (string.IsNullOrWhiteSpace(deviceId) || string.IsNullOrWhiteSpace(token))
        {
            return false;
        }

        return _biometricProfiles.TryGetValue(deviceId, out var profile) &&
               string.Equals(profile.Token, token, StringComparison.Ordinal);
    }

    public void RegisterBiometric(string deviceId, string token)
    {
        if (string.IsNullOrWhiteSpace(deviceId) || string.IsNullOrWhiteSpace(token))
        {
            throw new ArgumentException("DeviceId and token are required.");
        }

        var profile = new BiometricProfile
        {
            DeviceId = deviceId,
            Token = token
        };

        _biometricProfiles[deviceId] = profile;
    }

    VaultSetupState GetStateUnsafe()
        => new()
        {
            IsConfigured = _pinHash is not null,
            PinConfigured = _pinHash is not null,
            MasterPasswordConfigured = _passwordHash is not null,
            BiometricPreferred = _biometricPreferred
        };
}

