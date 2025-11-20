namespace Dadstart.Labs.Crow.ViewModels;

using CommunityToolkit.Mvvm.Messaging;
using Dadstart.Labs.Crow.Abstractions;
using Dadstart.Labs.Crow.Messages;
using Dadstart.Labs.Crow.Security;
using Dadstart.Labs.Crow.Services.Security;

public partial class StartupViewModel : ObservableObject
{
    readonly IVaultApiClient _apiClient;
    readonly VaultSessionState _sessionState;
    readonly IDeviceSecurityService _deviceSecurity;
    readonly IMessenger _messenger;

    bool _initialized;

    public StartupViewModel(
        IVaultApiClient apiClient,
        VaultSessionState sessionState,
        IDeviceSecurityService deviceSecurity,
        IMessenger messenger)
    {
        _apiClient = apiClient;
        _sessionState = sessionState;
        _deviceSecurity = deviceSecurity;
        _messenger = messenger;

        InitializeCommand = new AsyncRelayCommand(InitializeAsync);
        SetupCommand = new AsyncRelayCommand(SetupAsync, () => !IsBusy);
        UnlockWithPinCommand = new AsyncRelayCommand(UnlockWithPinAsync, () => !IsBusy);
        UnlockWithPasswordCommand = new AsyncRelayCommand(UnlockWithPasswordAsync, () => !IsBusy);
        UnlockWithBiometricCommand = new AsyncRelayCommand(UnlockWithBiometricAsync, () => !IsBusy);
    }

    public IAsyncRelayCommand InitializeCommand { get; }

    public IAsyncRelayCommand SetupCommand { get; }

    public IAsyncRelayCommand UnlockWithPinCommand { get; }

    public IAsyncRelayCommand UnlockWithPasswordCommand { get; }

    public IAsyncRelayCommand UnlockWithBiometricCommand { get; }

    [ObservableProperty]
    bool isBusy;

    [ObservableProperty]
    bool requiresSetup;

    [ObservableProperty]
    bool supportsBiometric;

    [ObservableProperty]
    bool biometricPreferred;

    [ObservableProperty]
    bool unlockEnabled;

    [ObservableProperty]
    string statusMessage = string.Empty;

    [ObservableProperty]
    string setupPin = string.Empty;

    [ObservableProperty]
    string? setupMasterPassword;

    [ObservableProperty]
    bool setupBiometric = true;

    [ObservableProperty]
    string unlockPin = string.Empty;

    [ObservableProperty]
    string? unlockPassword;

    async Task InitializeAsync()
    {
        if (_initialized || IsBusy)
        {
            return;
        }

        await ExecuteAsync(async () =>
        {
            SupportsBiometric = await _deviceSecurity.IsBiometricAvailableAsync();
            var state = await _apiClient.GetSetupStateAsync();
            RequiresSetup = !state.IsConfigured;
            BiometricPreferred = state.BiometricPreferred;
            UnlockEnabled = state.IsConfigured;
            _initialized = true;
        });
    }

    async Task SetupAsync()
    {
        if (string.IsNullOrWhiteSpace(SetupPin) || SetupPin.Length < 4)
        {
            StatusMessage = "PIN must have at least 4 digits.";
            return;
        }

        await ExecuteAsync(async () =>
        {
            var request = new VaultSetupRequest
            {
                Pin = SetupPin,
                MasterPassword = string.IsNullOrWhiteSpace(SetupMasterPassword) ? null : SetupMasterPassword,
                EnableBiometric = SetupBiometric && SupportsBiometric
            };

            var state = await _apiClient.ConfigureAsync(request);
            RequiresSetup = !state.IsConfigured;
            BiometricPreferred = state.BiometricPreferred;
            UnlockEnabled = state.IsConfigured;
            StatusMessage = "Vault ready. Unlock to continue.";
        });
    }

    Task UnlockWithPinAsync()
    {
        if (string.IsNullOrWhiteSpace(UnlockPin))
        {
            StatusMessage = "Enter your PIN.";
            return Task.CompletedTask;
        }

        var request = new UnlockRequest
        {
            Method = AuthenticationMethod.Pin,
            Pin = UnlockPin
        };

        return UnlockAsync(request);
    }

    Task UnlockWithPasswordAsync()
    {
        if (string.IsNullOrWhiteSpace(UnlockPassword))
        {
            StatusMessage = "Enter your master password.";
            return Task.CompletedTask;
        }

        var request = new UnlockRequest
        {
            Method = AuthenticationMethod.MasterPassword,
            MasterPassword = UnlockPassword
        };

        return UnlockAsync(request);
    }

    async Task UnlockWithBiometricAsync()
    {
        if (!SupportsBiometric)
        {
            StatusMessage = "Biometric login unavailable.";
            return;
        }

        var storedToken = await _deviceSecurity.GetPersistedBiometricTokenAsync();
        if (string.IsNullOrWhiteSpace(storedToken))
        {
            StatusMessage = "Enroll biometrics after unlocking once.";
            return;
        }

        var success = await _deviceSecurity.AuthenticateWithBiometricsAsync("Unlock Crow Vault");
        if (!success)
        {
            StatusMessage = "Biometric authentication failed.";
            return;
        }

        var request = new UnlockRequest
        {
            Method = AuthenticationMethod.Biometric,
            BiometricToken = storedToken
        };

        await UnlockAsync(request);
    }

    async Task UnlockAsync(UnlockRequest request)
    {
        await ExecuteAsync(async () =>
        {
            request = request with
            {
                DeviceId = await _deviceSecurity.GetOrCreateDeviceIdAsync()
            };

            var response = await _apiClient.UnlockAsync(request);
            _sessionState.StartSession(response);

            if (response.BiometricAllowed && SupportsBiometric)
            {
                await EnsureBiometricEnrollmentAsync(response.SessionToken);
            }

            StatusMessage = string.Empty;
            _messenger.Send(new VaultUnlockedMessage());
        });
    }

    async Task EnsureBiometricEnrollmentAsync(string sessionToken)
    {
        var storedToken = await _deviceSecurity.GetPersistedBiometricTokenAsync();
        if (!string.IsNullOrEmpty(storedToken))
        {
            return;
        }

        var granted = await _deviceSecurity.AuthenticateWithBiometricsAsync("Enable biometrics for faster unlocks");
        if (!granted)
        {
            return;
        }

        var token = Guid.NewGuid().ToString("N");
        await _deviceSecurity.PersistBiometricTokenAsync(token);
        var deviceId = await _deviceSecurity.GetOrCreateDeviceIdAsync();
        await _apiClient.RegisterBiometricAsync(sessionToken, deviceId, token);
    }

    async Task ExecuteAsync(Func<Task> action)
    {
        if (IsBusy)
        {
            return;
        }

        try
        {
            IsBusy = true;
            StatusMessage = string.Empty;
            await action();
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
            SetupCommand.NotifyCanExecuteChanged();
            UnlockWithPinCommand.NotifyCanExecuteChanged();
            UnlockWithPasswordCommand.NotifyCanExecuteChanged();
            UnlockWithBiometricCommand.NotifyCanExecuteChanged();
        }
    }

    partial void OnRequiresSetupChanged(bool value)
    {
        UnlockEnabled = !value;
    }
}

