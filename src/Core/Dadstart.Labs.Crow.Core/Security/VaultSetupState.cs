namespace Dadstart.Labs.Crow.Security;

public sealed record class VaultSetupState
{
    public bool IsConfigured { get; init; }

    public bool PinConfigured { get; init; }

    public bool MasterPasswordConfigured { get; init; }

    public bool BiometricPreferred { get; init; }
}

