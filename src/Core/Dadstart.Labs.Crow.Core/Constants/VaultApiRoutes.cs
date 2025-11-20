namespace Dadstart.Labs.Crow.Constants;

public static class VaultApiRoutes
{
    public const string ApiBase = "/api";

    public const string Setup = $"{ApiBase}/setup";

    public const string SetupState = $"{Setup}/state";

    public const string Auth = $"{ApiBase}/auth";

    public const string Unlock = $"{Auth}/unlock";

    public const string Biometric = $"{Auth}/biometric";

    public const string Notes = $"{ApiBase}/notes";

    public const string Passwords = $"{ApiBase}/passwords";

    public const string Reminders = $"{ApiBase}/reminders";

    public const string SessionHeader = "X-Vault-Session";
}

